using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using ENet;
using MessagePack;

namespace AuthServer {
    class Program {
        public static List<PlayerInfo> playerList = new List<PlayerInfo> ();

        public static ConcurrentQueue<Tuple<Data, Peer>> queue = new ConcurrentQueue<Tuple<Data, Peer>> ();
        public static ConcurrentQueue<String> logQueue = new ConcurrentQueue<String> ();
        static bool isRunning = false;

        static float clientTickRate = 0.03f;

        static void Main (string[] args) {

            if (!isRunning) {
                isRunning = true;
                Thread logger = new Thread (new ThreadStart (LoggerProcessor));
                Thread mainListener = new Thread (new ThreadStart (MainListener));
                Thread processQueue = new Thread (new ThreadStart (ProcessQueue));
                Thread broadcastPositions = new Thread (new ThreadStart (BroadcastPositions));

                logger.Start ();
                mainListener.Start ();
                processQueue.Start ();
                broadcastPositions.Start ();
            }

            ENet.Library.Deinitialize ();
        }

        static void MainListener () {
            Log ("Starting Server");
            if (ENet.Library.Initialize ()) {
                using (Host server = new Host ()) {
                    Address address = new Address ();
                    address.Port = 7995;
                    server.Create (address, 1000);

                    ENet.Event netEvent;

                    while (isRunning) {
                        bool polled = false;
                        while (!polled) {
                            if (server.CheckEvents (out netEvent) <= 0) {
                                if (server.Service (15, out netEvent) <= 0) {
                                    break;
                                }
                                polled = true;
                            }

                            switch (netEvent.Type) {
                                case ENet.EventType.None:
                                    //Do nothing
                                    break;
                                case ENet.EventType.Connect:
                                    //Connection attempt
                                    Log ("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);

                                    //Send connection accept message
                                    playerList.Add (new PlayerInfo () { client = netEvent.Peer });
                                    break;
                                case ENet.EventType.Disconnect:
                                    Log ("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);

                                    playerList.Remove (playerList.Where (x => x.client.ID.Equals (netEvent.Peer.ID)).First ());
                                    break;
                                case ENet.EventType.Timeout:
                                    Log ("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                    break;
                                case ENet.EventType.Receive:
                                    Log ("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);

                                    byte[] buffer = new byte[netEvent.Packet.Length];
                                    netEvent.Packet.CopyTo (buffer);
                                    netEvent.Packet.Dispose ();

                                    Data data = MessagePackSerializer.Deserialize<Data> (buffer);
                                    queue.Enqueue (new Tuple<Data, Peer> (data, netEvent.Peer));
                                    break;
                            }
                        }
                    }

                }
            }else{
                ENet.Library.Deinitialize();
                Log("Closing can't start server");
                isRunning = false;
            }
        }
    

        

        static void Send (Tuple<Data, Peer> tuple) {
            Packet packet = default (Packet);
            byte[] message = MessagePackSerializer.Serialize (tuple.Item1);
            packet.Create (message, PacketFlags.Reliable);
            tuple.Item2.Send (0, ref packet);
        }

        static void ProcessQueue () {
            while (isRunning) {
                if (queue.Count > 0) {
                    Tuple<Data, Peer> data;
                    queue.TryDequeue (out data);
                    Data newMsg;

                    if (data == null) {
                        Log ("data is null uh oh");
                    }

                    switch (data.Item1.command) {
                        case Command.LOGIN:
                            playerList.Where (x => x.client.Equals (data.Item2)).First ().userId = data.Item1.clientId;
                            break;
                        case Command.MOVEMENT_UPDATE:
                            PlayerInfo pi = playerList.Where (x => x.client.Equals (data.Item2)).First ();
                            Vector3 newPos = Vector3.Deserialise (data.Item1.content);
                            if ((((newPos - pi.lastPosition).magnitude ()) / clientTickRate) <= pi.speed) {
                                playerList.Where (x => x.client.Equals (data.Item2)).First ().lastPosition = newPos;
                            } else {
                                Log (pi.userId + " has moved further than they should");
                                float distance = pi.speed * clientTickRate;
                                Vector3 directionTravel = newPos - pi.lastPosition;
                                Vector3 finalDirection = directionTravel + (directionTravel.normalized () * distance);
                                Vector3 targetPosition = pi.lastPosition + finalDirection;
                                playerList.Where (x => x.client.Equals (data.Item2)).First ().lastPosition = targetPosition;
                            }
                            break;
                        case Command.DISCONNECT:

                            break;
                        case Command.TEST_MESSAGE:
                            Log ("Received test message from " + data.Item1.clientId + " : " + data.Item1.content);
                            newMsg = new Data () { command = Command.TEST_MESSAGE, content = "Cheers we received it" };
                            Send (new Tuple<Data, Peer> (newMsg, data.Item2));
                            break;
                        case Command.EXTERNAL_PONG:

                            break;
                    }

                }
            }
        }

        static void BroadcastPositions () {
            while (isRunning) {

                Data message = new Data () {
                    command = Command.PLAYER_POSITION_UPDATE,
                    content = "",
                    sendTime = DateTime.UtcNow,
                    positions = new List<string> ()
                };

                foreach (PlayerInfo pi in playerList) {
                    message.positions.Add (pi.userId + "/" + pi.lastPosition.x + "/" + pi.lastPosition.y + "/" + pi.lastPosition.z);
                }

                foreach (PlayerInfo pi in playerList) {
                    Send (new Tuple<Data, Peer> (message, pi.client));
                }

                Thread.Sleep (100);
            }
        }

        static void Log (string message) {
            logQueue.Enqueue (message);
        }

        static void LoggerProcessor () {
            while (isRunning) {
                if (logQueue.Count > 0) {
                    string temp = "";
                    logQueue.TryDequeue (out temp);
                    Console.WriteLine (temp);
                }
            }
        }
    }
}