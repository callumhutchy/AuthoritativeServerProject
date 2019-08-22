using System;
using System.Net;
using System.Net.Sockets;

namespace AuthServer
{
    public class PlayerInfo
    {
        public IPEndPoint clientEP;
        public int userId;
        public float speed;
        public Vector3 lastPosition;
    }
}