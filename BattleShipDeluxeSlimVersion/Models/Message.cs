using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipDeluxeSlimVersion.Models
{
    public class Message
    {
        public string Text { get; set; }
        public bool TextHasMultipleLines { get; set; }
        public bool MessageLedToGameEnd { get; set; }        
    }
}
