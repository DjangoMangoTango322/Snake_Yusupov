using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using Common;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Threading;
using System.Xml.Linq;

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
        private static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                Points = new List<Snakes.Point>() { 
                    new Snakes.Point(30, 10),
                    new Snakes.Point(20, 10),
                    new Snakes.Point(10, 10)
                },
                direction = Snakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Snakes.Point(
                new Random().Next(10, 783),
                new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }
        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);
                List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver == true);
                if (RemoteSnakes.Count > 0)
                {
                    foreach (ViewModelGames DeadSnake in RemoteSnakes) { 
                        ViewModelUserSettings User = remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Отклечил пользоmателя: {User.IpAddress}:{User.Port}"); 
                        remoteIPAddress.Remove(User);
                    }
                    viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver == true);
                    foreach (ViewModelUserSettings User in remoteIPAddress) { 
                        Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                        for (int iSnake = Snake.Points.Count - 1; iSnake >= 0; iSnake--)
                        {
                            if(iSnake != 0)
                            {
                                Snake.Points[iSnake] = Snake.Points[iSnake - 1];
                            }
                            else
                            {
                                int Speed = 10 + (int)Math.Round(Snake.Points.Count / 20f);
                                if (Speed > MaxSpeed) Speed = MaxSpeed;
                                if (Snake.direction == Snakes.Direction.Right)
                                    Snake.Points[iSnake] = new Snakes.Point(Snake.Points[iSnake].X + Speed, Snake.Points[iSnake].Y); 
                                if (Snake.direction == Snakes.Direction.Down)
                                    Snake.Points[iSnake] = new Snakes.Point(Snake.Points[iSnake].X, Snake.Points[iSnake].Y + Speed);
                                if (Snake.direction == Snakes.Direction.Up)
                                    Snake.Points[iSnake] = new Snakes.Point(Snake.Points[iSnake].X, Snake.Points[iSnake].Y - Speed);
                                if (Snake.direction == Snakes.Direction.Left)
                                    Snake.Points[iSnake] = new Snakes.Point(Snake.Points[iSnake].X - Speed, Snake.Points[iSnake].Y);
                                if (Snake.Points[0].x <= || Snake.Points[0].x >= 793)
                                    Snake.GameOver = true;
                                if(Snake.Points[0].Y <= 0 || Snake.Points[0].Y >= 420)
                                    Snake.GameOver = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
