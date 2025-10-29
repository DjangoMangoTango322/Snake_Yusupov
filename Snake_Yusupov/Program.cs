using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using Common;
using System.Text;
using System;
using Newtonsoft.Json;

namespace Snake_Yusupov
{
    public class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        public static int LocalPort = 5001;
        public static int Speed = 15;
        static void Main(string[] args)
        {
        }
        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                IPAddress.Parse(User.IpAddress),
                int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake)));
                    sender.Send(bytes, bytes.Length, endPoint);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Отправил данные пользователю: {User.IpAddress}:{User.Port}");
                }
                catch (Exception exp )
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + exp.Message );
                }
                finally
                {
                    sender.Close();
                }
                
            }
        }
        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient();
            IPEndPoint endPoint = null;
            try
            {
                Console.WriteLine("Команды сервера: ");
                while (true)
                {
                    byte[] receiverData = receivingUdpClient.Receive(ref endPoint);
                    string returnData = Encoding.UTF8.GetString(receiverData);
                    string[] dataMessage = returnData.Split('|');
                    ViewModelUserSettings User = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                    if (returnData.ToString().Contains("/start"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользоиатель: {User.IpAddress}:{User.Port}");
                        
                        
                        remoteIPAddress.Add(User);
                        User.IdSnake = AddSnake();
                        viewModelGames[User.IdSnake].IdSnake = User.IdSnake;
                    }
                    else
                    {
                        int IdPlayer = -1;
                        IdPlayer = remoteIPAddress.FindIndex(x => x.IpAddress == User.IpAddress && x.Port == User.Port);
                        if(IdPlayer != -1)
                        {
                            if(dataMessage[0] == "Up" && 
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                            else if (dataMessage[0] == "Down" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                            else if(dataMessage[0] == "Left" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                            else if (dataMessage[6] == "Right" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + exp.Message);
            }
        }

    }
}
