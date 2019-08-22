using System;
using ENet;
using MessagePack;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace AuthClient
{
    class Program
    {
        static string clientId;
        static bool isRunning;


        static void Main(string[] args)
        {
            clientId = IDGenerator.Generate();
            isRunning = true;
            ENet.Library.Initialize();

            Thread connectionLoop = new Thread(new ThreadStart(ConnectionLoop));
            Thread receiveQueue = new Thread(new ThreadStart(ReceiveProcess));

            connectionLoop.Start();
            receiveQueue.Start();

            while(isRunning){
                Console.WriteLine("Accepting input");

                string input = Console.ReadLine();

                Send(new Tuple<Data,Peer>(new Data(){command = Command.TEST_MESSAGE, content = input, clientId = clientId}, peer));
                

            }

        }

        static void Send(Tuple<Data,Peer> tuple){
            Packet packet = default(Packet);
            byte[] message = MessagePackSerializer.Serialize(tuple.Item1);
            packet.Create(message, PacketFlags.Reliable);
            tuple.Item2.Send(0, ref packet);
        }

        static Peer peer;
        static void ConnectionLoop(){
            using(Host client = new Host()){
                Address address = new Address();
                address.SetHost("127.0.0.1");
                address.Port = 7995;

                client.Create();
                peer = client.Connect(address);
                ENet.Event netEvent;

                while(isRunning){
                    bool polled = false;
                    while(!polled){
                        if(client.CheckEvents(out netEvent) <= 0){
                            if(client.Service(15, out netEvent) <= 0){
                                break;
                            }
                            polled = true;
                        }

                        switch(netEvent.Type){
                            case ENet.EventType.None:

                                break;
                            case ENet.EventType.Connect:
                                Console.WriteLine("We connected to the server");
                                break;
                            case ENet.EventType.Disconnect:
                                Console.WriteLine("We disconnected from the server");
                                break;
                            case ENet.EventType.Timeout:
                                Console.WriteLine("We timed out");
                                break;
                            case ENet.EventType.Receive:
                                Console.WriteLine("Received packet from server - Channel ID: " + netEvent.ChannelID + ", Data Length: " + netEvent.Packet.Length);
                                byte[] buffer = new byte[netEvent.Packet.Length];
                                netEvent.Packet.CopyTo(buffer);
                                netEvent.Packet.Dispose();
                                Data data = MessagePackSerializer.Deserialize<Data>(buffer);
                                receiveQueue.Enqueue(new Tuple<Data,Peer>(data,peer));
                                
                                break;
                        }

                    }
                }
            }
        }

        static ConcurrentQueue<Tuple<Data, Peer>> receiveQueue = new ConcurrentQueue<Tuple<Data, Peer>>();

        static void ReceiveProcess(){
            while(isRunning){
                while(receiveQueue.Count > 0){
                    Tuple<Data,Peer> tuple;
                    receiveQueue.TryDequeue(out tuple);

                    switch(tuple.Item1.command){
                        case Command.DETAIL_REQUEST:

                            break;
                        case Command.CONNECTED:

                            break;
                        case Command.NEW_PLAYER:

                            break;
                        case Command.PLAYER_POSITION_UPDATE:

                            break;
                        case Command.DISCONNECT:

                            break;
                        case Command.OPPONENT_DISCONNECT:

                            break;
                        case Command.INTERNAL_PING:

                            break;
                        case Command.NPC_UPDATE:
                    
                            break;
                        case Command.NPC_SPAWN:

                            break;
                        case Command.TEST_MESSAGE:
                            Console.WriteLine("Test message response received: " + tuple.Item1.content);
                            break;
                    }
                }
            }
        }

    }
}
