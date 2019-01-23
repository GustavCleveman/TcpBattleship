using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipDeluxeSlimVersion.Models
{
    public class Board
    {
        public Cell[,] Cells { get; set; }
        public List<Ship> Ships { get; set; }
        public int ShipsLeft { get; set; }
        public void Draw()
        {
            Console.Write(" ");
            for (int i = 1; i < 11; i++)
            {
                Console.Write($" {i} ");

            }
            Console.WriteLine();
            for (int i = 0; i < 10; i++)
            {
                Console.Write((char)(i + 65));
                for (int j = 1; j < 11; j++)
                {

                    if (Cells[i, j - 1].Status == Status.NoStatus)
                    {
                        if (Cells[i, j - 1].Ship == null)
                        {
                            Console.Write("{ }");
                        }
                        else
                        {
                            Console.Write("{o}");
                        }
                    }
                    else
                    {
                        if (Cells[i, j - 1].Status == Status.Hit)
                        {
                            Console.Write("{*}");

                        }
                        else
                        {
                            Console.Write("{#}");
                        }
                    }
                    if (j % 10 == 0)
                    {
                        Console.WriteLine();
                    }
                }
            }
        }
        public void UpdateBoard(int[] cords)
        {
            var status = Cells[cords[0], cords[1]].Status.ToString();
            Ship ship = Cells[cords[0], cords[1]].Ship;
            if (status == "NoStatus")
            {
                if (ship == null) { Cells[cords[0], cords[1]].Status = Status.Miss; }
                else
                {
                    Cells[cords[0], cords[1]].Status = Status.Hit;
                    Cells[cords[0], cords[1]].Ship.LivesLeft--;
                }
            }
        }
    }
}
