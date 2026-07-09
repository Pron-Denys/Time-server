namespace Time_server
{
    using System.Net;
    using System.Text;
    using System.Net.Sockets;
    public partial class Form1 : Form
    {
        string? ip_address;
        SynchronizationContext uiContext;
        Socket? sock;
        IPEndPoint? end_point;
        public Form1()
        {
            InitializeComponent();
            ip_address = null;
            uiContext = SynchronizationContext.Current!;
            sock = null;
            end_point = null;
        }

        private async void ReceiveFrom()
        {
            await Task.Run(() =>
            {
                try
                {
                    end_point = new IPEndPoint(IPAddress.Parse(ip_address!), 49152);
                    sock = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void SendTo()
        {
            await Task.Run(async () =>
            {
                try
                {
                    IPEndPoint end_point_1 = new IPEndPoint(IPAddress.Parse(ip_address!), 49154);
                    Socket sock_connect = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    sock_connect.SendTo(Encoding.Default.GetBytes("Connect"), end_point_1);
                    EndPoint remote_1 = new IPEndPoint(0x7F0000, 100);
                    byte[] buf_bytes_1 = new byte[1024];
                    int len = sock_connect.ReceiveFrom(buf_bytes_1, ref remote_1);

                    IPEndPoint end_point_2 = new IPEndPoint(IPAddress.Parse(ip_address!), 49153);
                    Socket sock = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    byte[] bytes = Encoding.Default.GetBytes("Connect");
                    sock.SendTo(bytes, end_point_2);
                    EndPoint remote = new IPEndPoint(0x07F00000, 2001);
                    while (true)
                    {
                        byte[] buf_bytes = new byte[1024];
                        int count_bytes = sock.ReceiveFrom(buf_bytes, ref remote);
                        bytes = new byte[count_bytes];
                        Array.Copy(buf_bytes, 0, bytes, 0, count_bytes);
                        string time = Encoding.Default.GetString(bytes);
                        uiContext.Send((parametr) => label2.Text = time, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void Click_ConnectToServer(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                ip_address = textBox1.Text;
                SendTo();
                ReceiveFrom();
            }
            else
                MessageBox.Show("Заповніть поле", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private async void Disconnect()
        {
            await Task.Run(() =>
            {
                byte[] bytes = Encoding.Default.GetBytes("Disconnect");
                sock?.SendTo(bytes, end_point!);
            });
        }

        private void Click_Disconnect(object sender, EventArgs e)
        {
            Disconnect();
        }
    }
}
