using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using AccurateTimeProviderLib;
using TimeProviderApi;

namespace TimeProviderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<long>();
            AccurateTimeProvider time = new AccurateTimeProvider();

            Thread statisticsThread = new Thread(new ParameterizedThreadStart(GetStat));
            statisticsThread.Start(time);
            //AccurateTimeProvider time = new AccurateTimeProvider();
            Thread.Sleep(30000);
            PrintCurrentTime(time);

        }
        private static long UdpConnect()
        {
            var stopwatch = new Stopwatch();
            const string ntpServer = "pool.ntp.org";
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            stopwatch.Start();
            socket.Receive(ntpData);
            stopwatch.Stop();
            socket.Close();
            long ts2 = stopwatch.ElapsedTicks;
            long ts1 = stopwatch.ElapsedMilliseconds;
            //Console.WriteLine(ts2);
            //Console.WriteLine("Время приёма");
            ulong intPart2 = (ulong)ntpData[32] << 24 | (ulong)ntpData[33] << 16 | (ulong)ntpData[34] << 8 | (ulong)ntpData[35];
            ulong fractPart2 = (ulong)ntpData[36] << 24 | (ulong)ntpData[37] << 16 | (ulong)ntpData[38] << 8 | (ulong)ntpData[39];
            var milliseconds2 = (intPart2 * 10000000) + ((fractPart2 * 10000000) / 0x100000000L);
            //Console.WriteLine(milliseconds2);

            //Console.WriteLine("Время отправки");
            ulong intPart1 = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart1 = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var microseconds1 = (intPart1 * 10000000) + ((fractPart1 * 10000000) / 0x100000000L);
            //Console.WriteLine(microseconds1);

            //Console.WriteLine("Интервал по ntp");
            ulong inter = microseconds1 - milliseconds2;
            //Console.WriteLine(inter);

            Console.WriteLine("Время в пути:");
            ulong timeofroad = ((ulong)ts2 - inter) / 2;
            Console.WriteLine(timeofroad);
            return (long)timeofroad;

        }
        public static void GetStat(object time)
        {
            AccurateTimeProvider timeProvider = (AccurateTimeProvider)time;
            timeProvider.GetStatistics();
        }
        

        
        private static void TcpConnectExample()
        {
	        using TcpClient client = new TcpClient();
	        client.Connect(IPAddress.Loopback, 123);

	        using var stream = client.GetStream();
	        stream.Write(null);
	        stream.Read(null);
        }

        private static void StopWatchExample()
        {
	        var stopwatch = new Stopwatch();
	        stopwatch.Restart(); 
	        stopwatch.Start();
	        Console.WriteLine(stopwatch.Elapsed);
        }

        private static void PrintCurrentTime(AccurateTimeProvider timeProvider)
        {
            //var list = new List<long>();
            //list=timeProvider.GetStatistics();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            timeProvider.ProstSred();
            stopwatch.Stop();
            long ts2 = stopwatch.ElapsedTicks;
            Console.WriteLine("Время");
            Console.WriteLine(ts2);
            stopwatch.Restart();
            timeProvider.NaimKvadr();
            stopwatch.Stop();
            ts2 = stopwatch.ElapsedTicks;
            Console.WriteLine("Время");
            Console.WriteLine(ts2);
            stopwatch.Restart();
            timeProvider.SkolSred();
            stopwatch.Stop();
            ts2 = stopwatch.ElapsedTicks;
            Console.WriteLine("Время");
            Console.WriteLine(ts2);
            stopwatch.Restart();
            timeProvider.ExpSglazh();
            stopwatch.Stop();
            ts2 = stopwatch.ElapsedTicks;
            Console.WriteLine("Время");
            Console.WriteLine(ts2);
            
        }
    }
}
