using System;

namespace TanatServer
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using AMF;
    using AmfData;

    namespace TcpListenerApp
    {
        class Program
        {
            const int port = 7777; // порт для прослушивания подключений
            static void Main(string[] args)
            {
                TcpListener server = null;
                try
                {
                    IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                    server = new TcpListener(localAddr, port);

                    // запуск слушателя
                    server.Start();

                    while (true)
                    {
                        Console.OutputEncoding = Encoding.UTF8;
                        Console.WriteLine("Ожидание подключений... ");

                        // получаем входящее подключение
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Подключен клиент. Выполнение запроса...");

                        // получаем сетевой поток для чтения и записи
                        NetworkStream stream = client.GetStream();


                        byte[] data = new byte[4];
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            do
                            {

                                stream.Read(data, 0, data.Length);
                                memoryStream.Write(data, 0, data.Length);

                            } while (stream.DataAvailable);
                            System.IO.File.WriteAllText(@"C:\Users\dailycode\test.amf", Encoding.ASCII.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));

                            Console.WriteLine(Encoding.ASCII.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));
                        }

                        // сообщение для отправки клиенту
                        //string response = "Привет мир";
                        // преобразуем сообщение в массив байтов
                        //byte[] data = Encoding.UTF8.GetBytes(response);

                        // отправка сообщения
                        //stream.Write(data, 0, data.Length);
                        //Console.WriteLine("Отправлено сообщение: {0}", response);
                        // закрываем поток
                        //stream.Close();
                        // закрываем подключение
                        //client.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    if (server != null)
                        server.Stop();
                }
            }
        }
    }
}
