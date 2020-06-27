using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class NtpClient
    {
        private const int SntpPort = 123;
        private static readonly DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public readonly string _nameServer;


        public NtpClient(string nameServer)
        {
            _nameServer = nameServer;
        }

        public async Task<DateTime> RequestTime()
        {
            var stopwatch = new Stopwatch();
            var ntpData = new byte[48];
            using (var udp = new UdpClient())
            {
                udp.Connect(_nameServer, SntpPort);
                ntpData[0] = 0x1B;
                await udp.SendAsync(ntpData, ntpData.Length);
                stopwatch.Start();
                var response = await udp.ReceiveAsync();
                stopwatch.Stop();
                ntpData = response.Buffer;
            }
            var timeOfRequest = stopwatch.ElapsedTicks;
            stopwatch.Restart();
            var intPart1 = (ulong)ntpData[32] << 24 | (ulong)ntpData[33] << 16 | (ulong)ntpData[34] << 8 | (ulong)ntpData[35];
            var fractPart1 = (ulong)ntpData[36] << 24 | (ulong)ntpData[37] << 16 | (ulong)ntpData[38] << 8 | (ulong)ntpData[39];
            var timeOfGet = (intPart1 * 10000000) + ((fractPart1 * 10000000) / 0x100000000L);

            var intPart2 = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            var fractPart2 = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var timeOfSend = (intPart2 * 10000000) + ((fractPart2 * 10000000) / 0x100000000L);

            if (timeOfSend == 0)
            {
                Thread.Sleep(500);
            }

            var timeOfProcess = timeOfSend - timeOfGet;
            var timeOfRoad = (timeOfRequest - (long)timeOfProcess) / 2;
            stopwatch.Stop();
            return BaseDate.AddTicks((long)timeOfSend + timeOfRoad + stopwatch.ElapsedTicks);
        }
    }
}