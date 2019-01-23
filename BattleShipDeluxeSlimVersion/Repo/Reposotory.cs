using System;
using System.Collections.Generic;
using System.Text;
using BattleShipDeluxeSlimVersion.Models;

namespace BattleShipDeluxeSlimVersion.Repo
{
    public static class Reposotory
    {
        public static List<string> ExistingReqCommands()
        {
            return new List<string>
            {
                "QUIT",
                "HELP",
                "STAR",
                "HELO",
                "FIRE"
            };
        }
        public static List<string> AllowedReqCommands(string state)
        {
            List<string> returnList = new List<string>();

            if (state == "WaitingForGreet")
            {
                returnList.Add("HELO");
            }
            else if (state == "WaitingForStart")
            {
                returnList.Add("STAR");
            }
            else if (state == "Playing")
            {
                returnList.Add("FIRE");
            }
            return returnList;
        }
        public static List<Ship> GetShips()
        {

            #region ships
            var q = new Ship
            {
                MaxLives = 5,
                LivesLeft = 5,
                Name = "Carrier",
                HitStatus = "241",
                SinkStatus = "251"
            };
            var w = new Ship
            {
                MaxLives = 4,
                LivesLeft = 4,
                Name = "Battleship ",
                HitStatus = "242",
                SinkStatus = "252"

            };
            var e = new Ship
            {
                MaxLives = 3,
                LivesLeft = 3,
                Name = "Submarine",
                HitStatus = "244",
                SinkStatus = "254"

            };
            var r = new Ship
            {
                MaxLives = 3,
                LivesLeft = 3,
                Name = "Patrol Boat",
                HitStatus = "245",
                SinkStatus = "255"

            };
            var t = new Ship
            {
                MaxLives = 2,
                LivesLeft = 2,
                Name = "Destroyer",
                HitStatus = "243",
                SinkStatus = "253"
            };
            #endregion
            return new List<Ship>() { q, w, e, r, t };
        }
        public static List<string> NextTurnResponses()
        {
            return new List<string>
            {
                "230",
                "241",
                "242",
                "243",
                "244",
                "245",
                "251",
                "252",
                "253",
                "254",
                "255"
            };
        }

    }
}
