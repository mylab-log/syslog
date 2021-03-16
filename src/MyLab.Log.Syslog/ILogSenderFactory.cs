namespace MyLab.Log.Syslog
{
    interface ILogSenderFactory
    {
        ILogSender Create(string hostname, int port);
    }
}