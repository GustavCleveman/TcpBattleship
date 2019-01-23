using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleShipDeluxeSlimVersion.Models;
using BattleShipDeluxeSlimVersion.Repo;


namespace BattleShipDeluxeSlimVersion.Business
{
    public static class ResponseHandler
    {
        public static Message ApplyToLocalGame(string myReq, string res, Game game)
        {
            try
            {
                res = res.Substring(0, 3);
            }
            catch (Exception e)
            {
                Console.WriteLine($"You did not recieve a valid response.\n '{e.Message}'");
                throw;
            }
            myReq = myReq.ToUpper();
            var returnThis = new Message();

            if (res == "260") { game.State++; }
            if (res == "270") { returnThis.MessageLedToGameEnd = true; }

            if (NextTurnResponses(res))
            {
                game.RemotPlayerTurn = !game.RemotPlayerTurn;
                int[] cords = ResponseFactory.ConvertToNumbers(myReq.Split(" ")[1]);
                if (res == "230") { game.Enemy.Board.Cells[cords[0], cords[1]].Status = Status.Miss; }
                else { game.Enemy.Board.Cells[cords[0], cords[1]].Status = Status.Hit; }
            }

            return returnThis;
        }

        private static bool NextTurnResponses(string res)
        {
            if (Reposotory.NextTurnResponses().FirstOrDefault(x => x == res) == null) { return false; }
            return true;
        }
    }
}
