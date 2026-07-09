namespace Program
{
    using System.Net;
    using System.Text;
    using System.Net.Sockets;
    using Time_Server;
    class Program
    {
        static void Main()
        {
            Time_Server time_server = new Time_Server();
            ConnectClients(time_server);
            Console.ReadLine();
        }

        static async void ConnectClients(Time_Server time_server)
        {
            await Task.Run(() =>
            {
                try
                {
                    IPEndPoint end_point = new IPEndPoint(IPAddress.Any, 49154);
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    sock.Bind(end_point);
                    while (true)
                    {
                        byte[] buf_bytes = new byte[1024];
                        EndPoint remote = new IPEndPoint(0x8F00000, 100);
                        int len = sock.ReceiveFrom(buf_bytes, ref remote);
                        byte[] bytes = new byte[len];
                        Array.Copy(buf_bytes, 0, bytes, 0, len);
                        string str = Encoding.UTF8.GetString(bytes);
                        if (str == "Connect")
                        {
                            int i = 0;
                            foreach (string connect in time_server.Connection_Log)
                            {
                                if (connect.IndexOf(((IPEndPoint)remote).Address.ToString()) != -1)
                                    break;
                                i += 1;
                            }
                            if (time_server.Connection_Log.Count == i)
                            {
                                time_server.Connected = false;
                                ReceiveFrom(time_server);
                                while (time_server.Connected == false)
                                {
                                    if (time_server.Connected)
                                        sock.SendTo(Encoding.Default.GetBytes("Connected"), (IPEndPoint)remote);
                                }
                            }
                            else
                                sock.SendTo(Encoding.Default.GetBytes("Continue"), (IPEndPoint)remote);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        static async void SendTo(Time_Server time_server, IPEndPoint remote_1)
        {
            await Task.Run(() =>
            {
                try
                {
                    IPEndPoint end_point = new(IPAddress.Any, 9152);
                    Socket sock = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    sock.Bind(end_point);
                    EndPoint remote_2 = new IPEndPoint(0xF0700000, 2003);
                    byte[] buf_bytes = new byte[1024];
                    int count_bytes = sock.ReceiveFrom(buf_bytes, ref remote_2);
                    byte[] bytes = new byte[count_bytes];
                    Array.Copy(buf_bytes, 0, bytes, 0, count_bytes);
                    string str = Encoding.Default.GetString(bytes);
                    if (str == "Disconnect")
                    {
                        Console.WriteLine($"Client: {((IPEndPoint)remote_2).Address}:{((IPEndPoint)remote_2).Port} disconnected {DateTime.Now}");
                        time_server.Connection_Log.Add($"Client: {((IPEndPoint)remote_2).Address}:{((IPEndPoint)remote_2).Port} disconnected {DateTime.Now}");
                        time_server.Disconnect = true;
                    }
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        static async void ReceiveFrom(Time_Server time_server)
        {
            await Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        time_server.Disconnect = false;
                        IPEndPoint end_point = new IPEndPoint(IPAddress.Any, 49153);
                        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        sock.Bind(end_point);
                        EndPoint remote = new IPEndPoint(0xF0700000, 2000);
                        byte[] buf_bytes = new byte[1024];
                        time_server.Connected = true;
                        int count_bytes = sock.ReceiveFrom(buf_bytes, ref remote);
                        Console.WriteLine($"Client: {((IPEndPoint)remote).Address}:{((IPEndPoint)remote).Port} connected {DateTime.Now}");
                        time_server.Connection_Log.Add($"Client: {((IPEndPoint)remote).Address}:{((IPEndPoint)remote).Port} connected {DateTime.Now}");
                        SendTo(time_server, (IPEndPoint)remote);
                        while (true)
                        {
                            if (time_server.Disconnect)
                            {
                                sock.Shutdown(SocketShutdown.Send);
                                sock.Close();
                                break;
                            }
                            Thread.Sleep(1000);
                            string time = $"{DateTime.Now}";
                            byte[] bytes = Encoding.Default.GetBytes(time);
                            sock.SendTo(bytes, (IPEndPoint)remote);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}

