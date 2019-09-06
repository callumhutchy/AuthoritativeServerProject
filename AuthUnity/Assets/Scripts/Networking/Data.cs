using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using System;


[MessagePackObject]
public class Data
    {
        [Key(0)]
        public Command command {get; set;}

        [Key(1)]
        public string content {get; set;}

        [Key(2)]
        public DateTime sendTime {get; set;}

        [Key(3)]
        public string clientId {get; set;}

        [Key(4)]
        public List<string> positions {get; set;}

    }

    public enum Command{
        DETAIL_REQUEST,
        LOGIN,
        CONNECTED,
        TEST_MESSAGE,
        MOVEMENT_UPDATE,
        PLAYER_POSITION_UPDATE,
        NEW_PLAYER,
        INTERNAL_PING,
        EXTERNAL_PONG,
        DISCONNECT,
        OPPONENT_DISCONNECT,
        NPC_UPDATE,
        NPC_SPAWN,
        NPC_DESPAWN
    }