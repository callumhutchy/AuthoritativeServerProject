using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using ENet;
using MessagePack;

namespace AuthServer
{
    class Program
    {
        public static List<PlayerInfo> playerList = new List<PlayerInfo>();

        public static List<Tuple<Data, Peer>> queue = new List<Tuple<Data,Peer>>();

        static bool isRunning = false;

        static void Main(string[] args)
        {
            

            if(!isRunning){
                isRunning = true;
                Thread mainListener = new Thread(new ThreadStart(MainListener));
                Thread processQueue = new Thread(new ThreadStart(ProcessQueue));
                Thread broadcastPositions = new Thread(new ThreadStart(BroadcastPositions));

                mainListener.Start();
                processQueue.Start();
                broadcastPositions.Start();
            }

            ENet.Library.Deinitialize();
        }

        static void MainListener(){
            Console.WriteLine("Starting Server");
            ENet.Library.Initialize();
            using(Host server = new Host()){
                Address address = new Address();
                address.Port = 7995;
                server.Create(address,1000);

                ENet.Event netEvent;

                while(isRunning){
                    bool polled = false;
                    while(!polled){
                        if(server.CheckEvents(out netEvent) <= 0){
                            if(server.Service(15,out netEvent) <= 0){
                                break;
                            }
                            polled = true;
                        }

                        switch(netEvent.Type){
                            case ENet.EventType.None:
                                //Do nothing
                                break;
                            case ENet.EventType.Connect:
                                //Connection attempt
                                Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);

                                //Send connection accept message

                                break;
                            case ENet.EventType.Disconnect:
                                Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                break;
                            case ENet.EventType.Timeout:
                                Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                break;
                            case ENet.EventType.Receive:
                                Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                
                                byte[] buffer = new byte[netEvent.Packet.Length];
                                netEvent.Packet.CopyTo(buffer);
                                netEvent.Packet.Dispose();

                                Data data = MessagePackSerializer.Deserialize<Data>(buffer);
                                
                                queue.Add(new Tuple<Data,Peer>(data,netEvent.Peer));
                                break;
                        }
                    }
                }

            }
        }

        static void Send(Tuple<Data, Peer> tuple){
            Packet packet = default(Packet);
            byte[] message = MessagePackSerializer.Serialize(tuple.Item1);
            packet.Create(message, PacketFlags.Reliable);
            tuple.Item2.Send(0,ref packet);
        }

        static void ProcessQueue(){
            while(isRunning){
                if(queue.Count > 0){
                    Tuple<Data, Peer> data = queue[0];
                    queue.RemoveAt(0);
                    Data newMsg;

                    if(data == null){
                        Console.WriteLine("data is null uh oh");
                    }

                    switch(data.Item1.command){
                        case Command.LOGIN:

                            break;
                        case Command.MOVEMENT_UPDATE:

                            break;
                        case Command.DISCONNECT:

                            break;
                        case Command.TEST_MESSAGE:
                            Console.WriteLine("Received test message: " + data.Item1.content);
                            newMsg = new Data(){command = Command.TEST_MESSAGE, content = "Cheers we received it"};
                            Send(new Tuple<Data,Peer>(newMsg,data.Item2));
                            break;
                        case Command.EXTERNAL_PONG:

                            break;
                    }

                }
            }
        }

        static void BroadcastPositions(){

        }
    }
}
