using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IDGenerator
    {

        private static char[] chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static System.Random _random = new System.Random();

        public static string Generate(int length = 6)
        {
            string id = "";
            for(int i = 0; i < length; i++)
            {
                id += chars[_random.Next(chars.Length)];
            }
            return id;
        }




    }
