﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Game_Server";
            Server.Start(10, 25932);
            Console.ReadKey();
        }
    }
}
