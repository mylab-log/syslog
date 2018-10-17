using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyLab.Syslog
{
    interface ILogSender
    {
        Task Send(string log);
    }

    class UdpLogSender : ILogSender
    {
        public string Hostname { get; }
        public int Port { get; }

        public UdpLogSender(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }
        
        public async Task  Send(string log)
        {
            var bin = Encoding.UTF8.GetBytes(log);

            var cl = new UdpClient();
            await cl.SendAsync(bin, bin.Length, Hostname, Port);
        }
    }

    class UdpLogSenderFactory : ILogSenderFactory
    {
        public ILogSender Create(string hostname, int port)
        {
            return new UdpLogSender(hostname, port);
        }
    }
}