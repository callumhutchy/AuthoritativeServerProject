using System;
using System.Net;
using System.Net.Sockets;
using ENet;

namespace AuthServer
{
    public class PlayerInfo
    {
        public Peer client;
        public String userId;
        public float speed = 1;
        public Vector3 lastPosition = new Vector3(0,0,0);
    }
}