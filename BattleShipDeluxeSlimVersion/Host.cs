using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BattleShipDeluxeSlimVersion.Business;
using BattleShipDeluxeSlimVersion.Models;

namespace BattleShipDeluxeSlimVersion
{
    public class Host
    {
        private TcpListener listener;
        private Game game;

        public void Run(int port)
        {
            StartListen(port);
            game = GameFactory.InitNewGame();
            game.Player.Name = GetPlayerName();
            while (true)
            {
                using (var client = listener.AcceptTcpClient())
                using (var networkStream = client.GetStream())
                using (var reader = new StreamReader(networkStream, Encoding.ASCII))
                using (var writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true })
                {
                    Console.WriteLine(client.Client.RemoteEndPoint.ToString() + " connected.");
                    writer.WriteLine("210 BATTLESHIP/1.0");
                    game.RemotPlayerTurn = true;

                    while (client.Connected)
                    {
                        try
                        {
                            //listening for req and sending res
                            while (game.RemotPlayerTurn && client.Connected)
                            {
                                string request = reader.ReadLine();
                                Message resp = ResponseFactory.BuildResponse(request, game, true);
                                if (resp.TextHasMultipleLines)
                                {
                                    writer.Write(resp.Text);
                                }
                                else { writer.WriteLine(resp.Text); }
                                DrawFrameClientTurn(request, resp.Text);
                                if (resp.MessageLedToGameEnd) { client.Close(); }
                            }

                            //sending req and waiting for res
                            while (!game.RemotPlayerTurn && client.Connected)
                            {
                                if (game.State == State.GameOver)
                                {
                                    game.RemotPlayerTurn = true;
                                    break;
                                }

                                Console.Write($"Your turn, {game.Player.Name}:");
                                //create and send request
                                string request = Console.ReadLine();
                                writer.WriteLine(request);
                                //recieve and handle response
                                string response = reader.ReadLine();
                                var mess = ResponseHandler.ApplyToLocalGame(request, response, game);
                                DrawFrameHostTurn(request, response);
                                if (mess.MessageLedToGameEnd) { client.Close(); }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                            throw;
                        }
                    }
                    Console.WriteLine($"Client,'{game.Enemy.Name}' disconnected...");
                    listener.Stop();
                    break;
                }
            }
        }

        private void StartListen(int port)
        {
            while (true)
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    Console.WriteLine($"Starts listening on port: {port}. Press start to enter name.");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failure to open socket.\n Most likely already in use.");
                    Console.Write("Please enter another port: ");
                    port = int.Parse(Console.ReadLine());
                }
            }
        }
        private string GetPlayerName()
        {
            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            if (name.Trim() == "") { name = "Host"; };
            return name;
        }
        private void DrawFrameClientTurn(string request, string response)
        {
            request += "      ";
            Console.Clear();
            game.Draw();
            if (request.ToUpper().Substring(0, 4) == "HELP")
            {
                Console.WriteLine($"{game.Enemy.Name ?? "Client"}" +
                $" said: '{request}'." +
                $"\nYou repsonded with your help data." +
                $"\nCurrent game state: {game.State.ToString()}");
            }
            else
            {
                Console.WriteLine($"{game.Enemy.Name ?? "Client"}" +
                  $" said: '{request}'.\nYou repsonded with '{response}" +
                  $"'.\nCurrent game state: {game.State.ToString()}");
            }
        }
        private void DrawFrameHostTurn(string request, string response)
        {
            Console.Clear();
            game.Draw();
            Console.WriteLine($"You said: '{request}'.\n{game.Enemy.Name ?? "Client"}repsonded with '{response}'.\nCurrent game state: {game.State.ToString()}");
        }
    }
}
