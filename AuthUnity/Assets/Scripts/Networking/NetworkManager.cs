using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using ENet;
using MessagePack;
using System.Threading;
using System;

public class NetworkManager : MonoBehaviour
{
    static string clientId;
    static bool isRunning;
    static bool connected = false;
    static bool logginIn = false;
    public GameObject player;

    List<string> newPositions = new List<string>();
    bool newPos = false;

    void Update(){
        if(newPos){
            List<string> tempPos = newPositions;
            
            foreach(string s in tempPos){
                Debug.Log(s);
                string[] split = s.Split('/');
                string id = split[0];
                Debug.Log(id);
                Vector3 pos = new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
                GameObject.Find(id).transform.position = pos;
            }
            newPos = false;
        }
    }

    public void Start()
    {
        clientId = IDGenerator.Generate();
        player.name = clientId;
        isRunning = true;

        Thread connectionLoop = new Thread(new ThreadStart(ConnectionLoop));
        Thread receiveQueue = new Thread(new ThreadStart(ReceiveProcess));

        Debug.Log("Starting threads");

        connectionLoop.Start();
        receiveQueue.Start();
    }

    public void Send(Tuple<Data, Peer> tuple)
    {
        if(connected || logginIn){
            Packet packet = default(Packet);
        byte[] message = MessagePackSerializer.Serialize(tuple.Item1);
        packet.Create(message, PacketFlags.Reliable);
        tuple.Item2.Send(0, ref packet);
        }
        
    }

    ConcurrentQueue<Tuple<Data, Peer>> receiveQueue = new ConcurrentQueue<Tuple<Data, Peer>>();

    private void OnApplicationQuit() {
         ENet.Library.Deinitialize();
         isRunning = false;
    }

    void FixedUpdate()
    {
        Vector3 pos = player.transform.position;
        if(connected){
            Send(new Tuple<Data, Peer>(new Data() { command = Command.MOVEMENT_UPDATE, content = pos.x + "/" + pos.y + "/" + pos.z, clientId = clientId }, peer));
        }
        
    }

    Peer peer;
    void ConnectionLoop()
    {
        if(ENet.Library.Initialize ()){

            Debug.Log("Init enet");
        using (Host client = new Host())
        {
            Address address = new Address();
            address.SetHost("127.0.0.1");
            address.Port = 7995;
            Debug.Log("Creating client");
            client.Create();
            peer = client.Connect(address);
            Debug.Log("Peer created");
            ENet.Event netEvent;

            
        while(isRunning){
                bool polled = false;
                while (!polled)
                {
                    
                    if (client.CheckEvents(out netEvent) <= 0)
                    {
                        if (client.Service(15, out netEvent) <= 0)
                        {
                            break;
                        }
                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case ENet.EventType.None:

                            break;
                        case ENet.EventType.Connect:
                            
                            Debug.Log("We connected to the server");
                            logginIn = true;
                            Send(new Tuple<Data,Peer>(new Data(){command = Command.LOGIN, clientId = clientId},peer));
                            
                            break;
                        case ENet.EventType.Disconnect:
                            Debug.Log("We disconnected from the server");
                            break;
                        case ENet.EventType.Timeout:
                            Debug.Log("We timed out");
                            break;
                        case ENet.EventType.Receive:
                            Debug.Log("Received packet from server - Channel ID: " + netEvent.ChannelID + ", Data Length: " + netEvent.Packet.Length);
                            byte[] buffer = new byte[netEvent.Packet.Length];
                            netEvent.Packet.CopyTo(buffer);
                            netEvent.Packet.Dispose();
                            Data data = MessagePackSerializer.Deserialize<Data>(buffer);
                            Debug.Log(data.command);
                            receiveQueue.Enqueue(new Tuple<Data, Peer>(data, peer));

                            break;
                    }

                }
            }
        }
        }else{
            Debug.Log("Cant init enet");
        }
    }

    void ReceiveProcess()
    { 
        while(isRunning){
        
                while(receiveQueue.Count > 0){
                    Tuple<Data,Peer> tuple;
                    receiveQueue.TryDequeue(out tuple);

                    switch(tuple.Item1.command){
                        case Command.DETAIL_REQUEST:

                            break;
                        case Command.CONNECTED:
                            connected = true;
                            break;
                        case Command.NEW_PLAYER:

                            break;
                        case Command.PLAYER_POSITION_UPDATE:
                            newPositions = tuple.Item1.positions;
                            newPos = true;
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
                            Debug.Log("Test message response received: " + tuple.Item1.content);
                            break;
                    }
                }
            }
    }
}
