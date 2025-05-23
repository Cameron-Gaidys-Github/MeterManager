using System.Net.NetworkInformation;

namespace MeterManager.Helpers
{
    public static class NetworkStatusChecker
    {
        public static bool IsPingSuccessful(string ipAddress)
        {
            try
            {
                using Ping ping = new();
                PingReply reply = ping.Send(ipAddress, 1000); // 1 second timeout
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
