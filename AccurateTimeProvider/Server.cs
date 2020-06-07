﻿using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    class Server
    {
        public String nameServer;
        public long timeofroad;
        public long neededtime;
        public long timeOfRequest;
        

        public Server(string nameServer)
        {
            this.nameServer = nameServer;
        }
       
        public void SNTP()
        {
            var stopwatch = new Stopwatch();
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            var addresses = Dns.GetHostEntry(nameServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            stopwatch.Start();
            socket.Receive(ntpData);
            stopwatch.Stop();
            socket.Close();
            timeOfRequest = stopwatch.ElapsedTicks;
            // long ts1 = stopwatch.ElapsedMilliseconds;
            //Console.WriteLine(ts2);
            //Console.WriteLine("Время приёма");
            ulong intPart2 = (ulong)ntpData[32] << 24 | (ulong)ntpData[33] << 16 | (ulong)ntpData[34] << 8 | (ulong)ntpData[35];
            ulong fractPart2 = (ulong)ntpData[36] << 24 | (ulong)ntpData[37] << 16 | (ulong)ntpData[38] << 8 | (ulong)ntpData[39];
            var timeOfGet = (intPart2 * 10000000) + ((fractPart2 * 10000000) / 0x100000000L);
            //Console.WriteLine(milliseconds2);

            //Console.WriteLine("Время отправки");
            ulong intPart1 = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart1 = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var timeOfSend = (intPart1 * 10000000) + ((fractPart1 * 10000000) / 0x100000000L);
            //Console.WriteLine(microseconds1);
            if (timeOfSend == 0)
            {
                Thread.Sleep(500);
            }
            //Console.WriteLine("Интервал по ntp");
            ulong inter = timeOfSend - timeOfGet;
            //Console.WriteLine(inter);

            //Console.WriteLine("Время");
            timeofroad = ((long)timeOfRequest - (long)inter) / 2;
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)(timeofroad + timeOfSend)).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            //return (long)(timeofroad + timeOfSend);
            neededtime = (long)(timeofroad + (long)timeOfSend);

        }
    }
}