namespace MyLab.Syslog
{
    /// <summary>
    /// Contains syslog writing options
    /// </summary>
    public class SyslogLoggerOptions
    {
        /// <summary>
        /// Overrides default hostname
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Overrides current application name
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Overrides default process id. Should be generated value.
        /// </summary>
        public string ProcId { get; set; }

        /// <summary>
        /// Gets syslog host
        /// </summary>
        public string RemoteHost { get; set; }

        /// <summary>
        /// Gets syslog port
        /// </summary>
        public int RemotePort { get; set; }

        /// <summary>
        /// Use TCP connection instead a UDP
        /// </summary>
        public bool UseTcp { get; set; } = false;

        /// <summary>
        /// Gets facility. Local7 - by default.
        /// </summary>
        /// <remarks>https://tools.ietf.org/html/rfc5424#section-6.2.1</remarks>
        public int Facility { get; set; } = 23;
    }
}