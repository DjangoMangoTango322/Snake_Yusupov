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
                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.Split('|');
                        ViewModelUserSettings User = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользоиатель: {User.IpAddress}:{User.Port}");
                        remoteIPAddress.Add(User);
                        User.IdSnake = AddSnake();
                        viewModelGames[User.IdSnake].IdSnake = User.IdSnake;
                    }
                    else
                    {

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
