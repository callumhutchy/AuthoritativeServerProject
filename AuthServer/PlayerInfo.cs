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
        public float speed;
        public Vector3 lastPosition;
    }
}