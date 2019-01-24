using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BattleShipDeluxeSlimVersion.Models;
using BattleShipDeluxeSlimVersion.Repo;

namespace BattleShipDeluxeSlimVersion.Business
{
    public static class ResponseFactory
    {

        public static Message BuildResponse(string request, Game game, bool isHost)
        {

            Message response = new Message
            {
                //unless changed later always false
                MessageLedToGameEnd = false,
            };

            if (request.Length < 4 && isHost)
            {
                game.BadRequests++;
                if (game.BadRequests > 2)
                {
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                }
                response.Text = "500 Syntax Error";
                return response;

            }

            if (request.Trim() == "")
            {
                response.Text = "500 Syntax Error";
                game.BadRequests++;
                return response;
            }
            if (!isHost) { return ClientResponses(request, game); }

            //DOUBLE MAKE SURE NO SPACES
            #region word splitters
            request = request.Trim();
            string noSpaceReq = Regex.Replace(request, @"\s+", "");
            if (noSpaceReq == "START") { noSpaceReq = "STAR"; }

            string firstWord = "";
            //is the first word too short? (no commands are shorter than 4 letters)
            try
            {
                firstWord = noSpaceReq.Substring(0, 4).ToUpper();
            }
            catch (Exception)
            {
                game.BadRequests++;
                if (game.BadRequests > 2)
                {
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                }

                response.Text = "500 Syntax Error";
                return response;
            }

            string secondWord = "";
            //contains name or cordinates?
            try
            {
                int length = request.Split(" ")[1].Length;
                secondWord = noSpaceReq.Substring(4, length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            #endregion

            //500 syntax error throwers
            if (Reposotory.ExistingReqCommands().FirstOrDefault(r => r == firstWord) == null)
            {
                game.BadRequests++;
                if (game.BadRequests > 2)
                {
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                }
                response.Text = "500 Syntax Error";
                return response;
            }

            //Always available requests.
            switch (firstWord)
            {
                case "QUIT":
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                case "HELP":
                    response.Text = GetHelp();
                    response.TextHasMultipleLines = true;
                    game.ResetBadRequest();
                    return response;
                default:
                    break;
            }

            //501 sequence error throwers
            if (Reposotory.AllowedReqCommands(game.State.ToString()).FirstOrDefault(r => r == firstWord) == null)
            {
                if (game.BadRequests > 2)
                {
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                }
                response.Text = "501 sequence Error";
                game.BadRequests++;
                return response;
            }

            //Fire 500 syntax error exception when cordinates wrong format
            if (firstWord == "FIRE" && !CordinatesValid(secondWord.ToUpper()))
            {
                game.BadRequests++;
                if (game.BadRequests > 2)
                {
                    response.Text = "270 Hasta la vista!";
                    response.MessageLedToGameEnd = true;
                    game.ResetBadRequest();
                    return response;
                }

                response.Text = "500 Syntax Error";
                return response;
            }

            //First successful req HELO
            if (firstWord == "HELO")
            {
                try { game.Enemy.Name = secondWord; }
                catch (Exception) { game.Enemy.Name = "Client"; }
                response.Text = $"220 {game.Player.Name}";
                game.State++;
                game.ResetBadRequest();
            }

            //start and dicethrow
            if (firstWord == "STAR")
            {
                var rnd = new Random();
                int num = rnd.Next(0, 11);
                if ((num % 2) != 0)
                {
                    response.Text = "221 You start";
                    game.State++;
                    game.ResetBadRequest();
                }
                else
                {
                    response.Text = "222 Host starts";
                    game.State++;
                    game.RemotPlayerTurn = !game.RemotPlayerTurn;
                    game.ResetBadRequest();
                }
            }

            //GAME TIMES
            if (firstWord == "FIRE")
            {
                game.ResetBadRequest();
                response = GetResponseFromPlayerBoard(ConvertToNumbers(secondWord.ToUpper()), game);
                if (response.Text.Substring(0, 3) == "260") { game.State++; };
            }

            return response;
        }
        private static string GetHelp()
        {
            //Stop it, Get some help... - Michael B Jordan
            return "**A\n" +
                "Write QUIT to terminate connection.\n" +
                "Write Fire<Coordinate> to fire.\n" +
                "IF opponent misses your boats, write '230 <Message>'.\n" +
                "If your opponent HIT your Carrier, write '241 <Message>'.\n" +
                "If your opponent HIT your Battleship, write '242 <Message>'.\n" +
                "If your opponent HIT your Destroyer, write '243 <Message>'.\n" +
                "If your opponent HIT your Submariner, write '244 <Message>'.\n" +
                "If your opponent HIT your Patrol Boat, write '245 <Message>'.\n" +
                "If your opponent SUNK your Carrier, write '251 <Message>'.\n" +
                "If your opponent SUNK your Battleship, write '252 <Message>'.\n" +
                "If your opponent SUNK your Destroyer, write '253 <Message>'.\n" +
                "If your opponent SUNK your Submariner, write '254 <Message>'.\n" +
                "If your opponent SUNK your Patrol Boat, write '255 <Message>'.\n" +
                "If your opponent wins, write '260 <Message>'\n" +
                "**\r";
        }
        private static bool CordinatesValid(string cords)
        {
            int[] returnInt = new int[2];

            var values = cords.ToCharArray();
            if (values.Length < 2 || values.Length > 3) { return false; }

            try
            {
                if (values.Length > 2)
                {
                    int firstNum = (int)values[0] - 65;
                    var secondNum = int.Parse(cords.Substring(cords.Length - 2)) - 1;
                    returnInt[0] = firstNum;
                    returnInt[1] = secondNum;
                }
                else
                {
                    int firstNum = (int)values[0] - 65;
                    var secondNum = char.GetNumericValue(cords, 1);
                    var secondNumINt = (int)secondNum - 1;
                    returnInt[0] = firstNum;
                    returnInt[1] = secondNumINt;
                }
                if (returnInt[0] < 0 || returnInt[0] >= 10)
                {
                    return false;
                }
                if (returnInt[1] < 0 || returnInt[1] >= 10)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static int[] ConvertToNumbers(string cords)
        {
            int[] returnInt = new int[2];
            if (cords.Length > 2)
            {
                int firstNum = cords[0] - 65;
                var secondNum = int.Parse(cords.Substring(cords.Length - 2)) - 1;
                returnInt[0] = firstNum;
                returnInt[1] = secondNum;
            }
            else
            {
                int firstNum = cords[0] - 65;
                var secondNum = char.GetNumericValue(cords, 1);
                var secondNumINt = (int)secondNum - 1;
                returnInt[0] = firstNum;
                returnInt[1] = secondNumINt;
            }
            return returnInt;
        }
        private static Message GetResponseFromPlayerBoard(int[] cords, Game game)
        {
            var status = game.Player.Board.Cells[cords[0], cords[1]].Status.ToString();
            Ship ship = game.Player.Board.Cells[cords[0], cords[1]].Ship;
            Message returnMessage = new Message();
            if (status == "NoStatus")
            {
                game.Player.Board.UpdateBoard(cords);
                if (ship == null)
                {
                    returnMessage.Text = "230 Miss!";
                    game.RemotPlayerTurn = !game.RemotPlayerTurn;
                }
                else
                {
                    if (ship.LivesLeft > 0)
                    {
                        returnMessage.Text = $"{ship.HitStatus} You hit my {ship.Name}.";
                        game.RemotPlayerTurn = !game.RemotPlayerTurn;
                    }
                    else
                    {
                        game.Player.Board.ShipsLeft--;
                        if (game.Player.Board.ShipsLeft == 0)
                        {
                            returnMessage.Text = "260 You win!";
                            //game over, stay clients turn and wait for either help or quit commands.

                        }
                        else
                        {
                            returnMessage.Text = $"{ship.SinkStatus} You sunk my {ship.Name}.";
                            game.RemotPlayerTurn = !game.RemotPlayerTurn;
                        }
                    }
                }
            }
            else
            {
                game.BadRequests++;
                if (game.BadRequests > 2)
                {
                    returnMessage.Text = "270 Hasta la vista!";
                }

                returnMessage.Text = "501 Sequence Order";

            }
            return returnMessage;
        }
        private static Message ClientResponses(string request, Game game)
        {
            Message response = new Message
            {
                //unless changed later always false
                MessageLedToGameEnd = false,
            };

            #region word splitters
            request = request.Trim();
            string noSpaceReq = Regex.Replace(request, @"\s+", "");
            if (noSpaceReq == "START") { noSpaceReq = "STAR"; }

            string firstWord = "";
            //is the first word to short? (no commands are shorter than 4 letters)
            try
            {
                firstWord = noSpaceReq.Substring(0, 4).ToUpper();
            }
            catch (Exception)
            {
                response.Text = "500 Syntax Error";
                return response;
            }

            string secondWord = "";
            //contains name or cordinates?
            try
            {
                int length = request.Split(" ")[1].Length;
                secondWord = noSpaceReq.Substring(4, length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            #endregion

            //Fire 500 syntax error exception when cordinates wrong format
            if (firstWord == "FIRE" && !CordinatesValid(secondWord.ToUpper()))
            {
                response.Text = "500 Syntax Error";
                game.BadRequests++;
                return response;
            }
            //hit, miss, sink or win
            if (firstWord == "FIRE")
            {
                if (game.State != State.Playing)
                {
                    response.Text = "501 Sequence Error";
                    game.BadRequests++;
                    return response;
                }
                response = GetResponseFromPlayerBoard(ConvertToNumbers(secondWord.ToUpper()), game);
                if (response.Text.Substring(0, 3) == "260") { game.State++; };
                return response;
            }

            //TODO ADD TO MANY BAD REQUEST
            response.Text = "500 Syntax Error";
            game.BadRequests++;
            return response;
        }
    }
}
