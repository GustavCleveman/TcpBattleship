using System;
using System.Collections.Generic;

using System.Text;
using BattleShipDeluxeSlimVersion.Models;
using BattleShipDeluxeSlimVersion.Repo;

namespace BattleShipDeluxeSlimVersion.Business
{
    public static class GameFactory
    {
        public static Game InitNewGame()
        {
            //setup local player
            Player localPlayer = new Player()
            {
                Name = Console.ReadLine(),
                Board = CreateBoard(),                
            };
            localPlayer.Board.Ships = GetShips();
            localPlayer.Board.ShipsLeft = 1;
            AutoPlaceShips(localPlayer);

            //setup remote player
            Player remotePlayer = new Player()
            {
                Name = "",
                Board = CreateBoard()
            };

            //setup return game
            Game game = new Game()
            {
                Player = localPlayer,
                Enemy = remotePlayer,
                State = State.WaitingForGreet,
            };
            return game;

        }

        //Creates Blank Board
        private static Board CreateBoard()
        {
            Board board = new Board()
            {
                Cells = new Cell[10, 10],
            };
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board.Cells[i, j] = new Cell();
                }
            }
            return board;
        }

        /*returns list of ships.
         only needed for local player*/
        private static List<Ship> GetShips()
        {
            return Reposotory.GetShips();
        }

        //needs input player to have instanciated board.
        private static void AutoPlaceShips(Player player)
        {
            try
            {
                var verticalIndex = 0;
                foreach (var ship in player.Board.Ships)
                {
                    for (int i = 0; i < ship.MaxLives; i++)
                    {
                        player.Board.Cells[verticalIndex, i + verticalIndex].Ship = ship;
                    }
                    verticalIndex++;
                    verticalIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong..\n '{ex.Message}'");
                throw;
            }
        }
    }
}

