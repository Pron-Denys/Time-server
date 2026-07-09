namespace Time_Server
{
    public class Time_Server
    {
        public bool Disconnect { get; set; }
        public bool Connected { get; set; }
        public List<string> Connection_Log { get; set; }

        public Time_Server()
        {
            Disconnect = false;
            Connection_Log = new List<string>();
        }
    }
}
