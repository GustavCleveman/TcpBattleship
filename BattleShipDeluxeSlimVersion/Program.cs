using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BattleShipDeluxeSlimVersion
{
    class Program
    {
        static string host;
        static int port;

        static void Main(string[] args)
        {
            SetUpIp();
            SetUpPort();
            Start();
        }

        static void Start()
        {
            if (host != "")
            {
                Client client = new Client();
                client.Run(host, port);
            }
            else
            {
                while (true)
                {
                    Host host = new Host();
                    host.Run(port);
                }
            }
        }
        static void SetUpIp()
        {
            bool ipValid = false;
            while (!ipValid)
            {
                Console.Write("Enter host ip or leave empty to host game: ");
                host = Console.ReadLine();
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                Match result = ip.Match(host);

                if (host == "")
                {
                    ipValid = true;
                    Console.WriteLine($"You are now hosting at {GetLocalIPAddress()}.");
                }
                else
                {
                    if (result.Success)
                    {
                        ipValid = true;
                        Console.WriteLine($"Connection set to ip: {host}.");
                    }
                    else
                    {
                        Console.WriteLine("Not a valid Ip-adress.");
                    }
                }
            }
        }
        static void SetUpPort()
        {
            while (true)
            {
                try
                {
                    Console.Write("Enter Port: ");
                    port = int.Parse(Console.ReadLine());
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

    }
}

