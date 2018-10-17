namespace MyLab.Syslog
{
    interface ILogSenderFactory
    {
        ILogSender Create(string hostname, int port);
    }
}