using BattleShipDeluxeSlimVersion.Business;
using BattleShipDeluxeSlimVersion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BattleShipDeluxeSlimVersion
{
    public class Client
    {
        Game game;
        public void Run(string host, int port)
        {
            game = GameFactory.InitNewGame();
            game.Player.Name = GetPlayerName();

            while (true)
            {
                using (var client = ConnectionStart(host, port))
                using (var networkStream = client.GetStream())
                using (StreamReader reader = new StreamReader(networkStream, Encoding.ASCII))
                using (var writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true })
                {
                    var line = reader.ReadLine();
                    Console.WriteLine(line);
                    #region massive client speed boot bs
                    while (true)
                    {
                        if (line == "210 BATTLESHIP/1.0")
                        {
                            writer.WriteLine($"HELO {game.Player.Name}");
                            line = reader.ReadLine();
                            Console.WriteLine(line);
                            if (line.Substring(0, 3) == "220")
                            {
                                try
                                {
                                    game.Enemy.Name = line.Split(" ")[1];
                                }
                                catch (Exception)
                                {
                                    game.Enemy.Name = "Host";
                                }
                            }
                            writer.WriteLine("START");
                            line = reader.ReadLine();
                            Console.WriteLine(line);
                            if (line.Substring(0, 3) == "221") { game.RemotPlayerTurn = false; }
                            else { game.RemotPlayerTurn = true; }
                            Console.WriteLine(game.RemotPlayerTurn.ToString());
                            game.State = State.Playing;
                            break;
                        }
                        else
                        {
                            client.Close();
                            break;
                        }
                    }
                    #endregion
                    while (client.Connected)
                    {
                        try
                        {
                            //listening for req and sending res
                            while (game.RemotPlayerTurn && client.Connected)
                            {
                                //client must always break connection
                                if (game.State == State.GameOver)
                                {
                                    game.RemotPlayerTurn = false;
                                    break;
                                }

                                string request = reader.ReadLine();
                                Message response = ResponseFactory.BuildResponse(request, game, false);
                                writer.WriteLine(response.Text);
                                DrawFrameHostTurn(request, response.Text);
                                if (response.MessageLedToGameEnd) { client.Close(); }
                            }

                            //sending req and waiting for res
                            while (!game.RemotPlayerTurn && client.Connected)
                            {
                                
                                Console.Write($"Your turn, {game.Player.Name}:");
                                //create and send request
                                string request = Console.ReadLine();
                                writer.WriteLine(request);

                                //Expect multiple rows of text
                                if (request.ToUpper().Trim() == "HELP") { GetHelp(reader); }

                                else
                                {
                                    //recieve and handle response
                                    string response = reader.ReadLine();
                                    var mess = ResponseHandler.ApplyToLocalGame(request, response, game);
                                    DrawFrameClientTurn(request, response);
                                    if (mess.MessageLedToGameEnd) { client.Close(); }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                            throw;
                        }
                    }
                    break;
                }
            }
        }

        private TcpClient ConnectionStart(string host, int port)
        {
            TcpClient client;
            while (true)
            {
                try
                {
                    client = new TcpClient(host, port);
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("please enter host again");
                    host = Console.ReadLine();
                    Console.WriteLine("please enter port again");
                    port = int.Parse(Console.ReadLine());
                }
            }
            return client;
        }
        private string GetPlayerName()
        {
            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            if (name.Trim() == "") { name = "Client"; };
            return name;
        }
        private void DrawFrameClientTurn(string request, string response)
        {
            Console.Clear();
            game.Draw();
            Console.WriteLine($"You said: '{request}'.\n{game.Enemy.Name ?? "Host"} repsonded with '{response}'.\nCurrent game state: {game.State.ToString()}\n ");
        }
        private void DrawFrameHostTurn(string request, string response)
        {
            Console.Clear();
            game.Draw();
            Console.WriteLine($"{game.Enemy.Name ?? "Host"} said: '{request}'.\nYou repsonded with '{response}'.\nCurrent game state: {game.State.ToString()}");
        }

        private void GetHelp(StreamReader reader)
        {
            var help = "";

            while (true)
            {
                char letter = (char)reader.Read();
                if (letter == '\r' || letter == 0) { break; }
                help += letter;

            }

            //while (true)
            //{
            //    string row = reader.ReadLine();
            //    if(row == null) { wasNullTwice++; }
            //    else { wasNullTwice = 0; }
            //    if(wasNullTwice == 3) { break; }
            //    help += row;

            //}

            Console.WriteLine(help);
            Console.WriteLine("press enter to return to game");
            Console.ReadKey();
            DrawFrameClientTurn("", "");

        }
    }
}



