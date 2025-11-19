using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using Common;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace Snake_Yusupov
{
    public class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        public static int LocalPort = 5001;
        public static int MaxSpeed = 15;
        static void Main(string[] args)
        {
            try
            {
                Thread thread = new Thread(new ThreadStart(Receiver));
                thread.Start();
                Thread threadTimer = new Thread(new ThreadStart(Timer));
                threadTimer.Start();
            }
            catch (Exception exp) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + exp.Message);
            }
        }
        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                IPAddress.Parse(User.IPAddress),
                int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake)));
                    sender.Send(bytes, bytes.Length, endPoint);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");
                }
                catch (Exception exp)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + exp.Message);
                }
                finally
                {
                    sender.Close();
                }

            }
        }
        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(LocalPort);
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
                        Console.WriteLine($"Подключился пользоиатель: {User.IPAddress}:{User.Port}");


                        remoteIPAddress.Add(User);
                        User.IdSnake = AddSnake();
                        viewModelGames[User.IdSnake].IdSnake = User.IdSnake;
                    }
                    else
                    {
                        int IdPlayer = -1;
                        IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == User.IPAddress && x.Port == User.Port);
                        if (IdPlayer != -1)
                        {
                            if (dataMessage[0] == "Up" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                            else if (dataMessage[0] == "Down" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                            else if (dataMessage[0] == "Left" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                            else if (dataMessage[6] == "Right" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                        }
                    }
                }
            }
            catch (Exception exp)
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
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        ViewModelUserSettings User = remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Отклечил пользоmателя: {User.IPAddress}:{User.Port}");
                        remoteIPAddress.Remove(User);
                    }
                    viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver == true);
                }
                foreach (ViewModelUserSettings User in remoteIPAddress) {
                    Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                    for (int iSnake = Snake.Points.Count - 1; iSnake >= 0; iSnake--)
                    {
                        if (iSnake != 0)
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

                        }
                    }
                    if (Snake.Points[0].X <= 0 || Snake.Points[0].X >= 793)
                        Snake.GameOver = true;
                    if (Snake.Points[0].Y <= 0 || Snake.Points[0].Y >= 420)
                        Snake.GameOver = true;
                    if (Snake.direction != Snakes.Direction.Start)
                    {
                        for (int iPoint = 1; iPoint < Snake.Points.Count; iPoint++)
                        {
                            if (Snake.Points[0].X >= Snake.Points[iPoint].X - 1 && Snake.Points[0].X <= Snake.Points[iPoint].X + 1)
                                if (Snake.Points[0].Y >= Snake.Points[iPoint].Y - 1 && Snake.Points[0].Y <= Snake.Points[iPoint].Y + 1)
                                    Snake.GameOver = true;
                        }
                    }
                    if (Snake.Points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                        Snake.Points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                    {
                        if (Snake.Points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                            Snake.Points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                        {
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(
                                new Random().Next(10, 783),
                                new Random().Next(10, 410));
                            Snake.Points.Add(new Snakes.Point(
                                Snake.Points[Snake.Points.Count - 1].X,
                                Snake.Points[Snake.Points.Count - 1].Y
                                ));
                            LoadLeaders();
                            Leaders.Add(new Common.Leaders()
                            {
                                Name = User.Name,
                                Points = Snake.Points.Count - 3

                            });
                            Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top =
                                Leaders.FindIndex(x => x.Points == Snake.Points.Count - 3 && x.Name == User.Name) + 1;
                        }
                    }
                    if (Snake.GameOver)
                    {
                        LoadLeaders();
                        Leaders.Add(new Common.Leaders()
                        {
                            Name = User.Name,
                            Points = Snake.Points.Count - 3
                        });
                        SaveLeaders();
                    }
                }
                Send();
            }
        }
    
        public static void SaveLeaders()
        {
            string Json = JsonConvert.SerializeObject(Leaders);
            StreamWriter sw = new StreamWriter("./leaders.txt");
            sw.WriteLine(Json);
            sw.Close();
        }
        public static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                StreamReader sr = new StreamReader("./leaders.txt");
                string Json = sr.ReadLine();
                sr.Close();
                if (!String.IsNullOrEmpty(Json)){
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(Json);
                }
                else Leaders = new List<Leaders>();
            }
            else
                Leaders = new List<Leaders>();
        }
    }
}
