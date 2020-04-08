using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Drawing;

using System.Threading;

namespace AccurateTimeProviderLib

{
    public class AccurateTimeProvider : ITimeProvider
    {
        DateTime ITimeProvider.Now => throw new NotImplementedException();
        public void checking()
        {
            Console.WriteLine("Всё будет хорошо");
        }

        /* public DateTime Now()
         {
             return DateTime.Now;
         }*/
        //=> throw new NotImplementedException();
        private static long Connect()
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

        public List<long> GetStatistics()
        {
            var list = new List<long>();
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine(i);
                list.Add(Connect());
                Thread.Sleep(5000);
            }
            
            return list;
            
        }

        
        //Метод простой средней
        public double ProstSred(List<long> list)
        {
            var stopwatch = new Stopwatch();
            double sum = 0;
            foreach(var i in list)
            {
                sum += i;

            }
            Console.WriteLine("Метод простой средней");
            Console.WriteLine(sum / 20);
            return sum / 20;

        }
        
        //Метод наименьших квадратов
        public void NaimKvadr(List<long> list)
        {
            long n =list.Count;
            long yf = 0;
            long x = 0;
            long yf_x = 0;
            long x2 = 0;
            long sum_x = 0;
            foreach(var i in list)
            {
                x += 1;
                yf += i;
                yf_x += i * x;
                x2 += x * x;
                sum_x +=x;

            }
            double a =((yf_x - (sum_x * yf)/n) / (x2 + (sum_x * sum_x)/n));
            double b = ((yf / n) - (a * sum_x / n));
            double result = b  + a*(n+1);
            Console.WriteLine("Метод наименьших квадратов");
            Console.WriteLine(result);
        }

        //Метод скользящей средней
        public void SkolSred(List<long> list)
        {
            int n = list.Count;
            double m = (list[n-3] + list[n-2] + list[n-1])/3;
            double result = m + (1 / 3) * (list[n-1] - list[n-2]);
            Console.WriteLine("Метод скользящей средней");
            Console.WriteLine(result);

        }

        //Метод экспоненциального сглаживания
        public void ExpSglazh(List<long> list)
        {
            double u_t = list[0];
            double n = list.Count;
            double alpha = 2 / (n+1);
            foreach(var i in list)
            {
                u_t = alpha * i + (1 - alpha) * u_t;
            }
            Console.WriteLine("Метод экспоненциального сглаживания");
            Console.WriteLine(u_t);
        }
    }
}