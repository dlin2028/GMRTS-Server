﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServer
{
    public class User
    {
        public string ID { get; set; }

        public Game CurrentGame { get; set; }

        public User(string id)
        {
            ID = id;
        }
    }
}