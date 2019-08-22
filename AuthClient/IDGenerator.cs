using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    class IDGenerator
    {

        private static char[] chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static Random _random = new Random();

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

