using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BattleShipDeluxeSlimVersion.Models
{
    public class Ship
    {
        public string Name { get; set; }
        public int MaxLives { get; set; }
        public int LivesLeft { get; set; }
        public string HitStatus { get; set; }
        public string SinkStatus { get; set; }
    }
}