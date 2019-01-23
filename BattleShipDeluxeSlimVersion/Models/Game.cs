using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipDeluxeSlimVersion.Models
{
    public class Game
    {
        public Player Player { get; set; }
        public Player Enemy { get; set; }
        public State State { get; set; }
        public int BadRequests { get; set; }
        public bool RemotPlayerTurn { get; set; }

        public void ResetBadRequest() {
            BadRequests = 0;
        }
        

        public void Draw()
        {
            Enemy.Board.Draw();
            Player.Board.Draw();
        }
    }
}
