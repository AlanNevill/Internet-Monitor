using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace InternetMonitor
{
    internal class Program
    {
        private static string _internetStateNow = "starting monitor";
        private static string _internetStatePrev;
        private static DateTime _dt = DateTime.Now;

        private static FileInfo _fileInfo = new FileInfo(@".\run.txt");
        private static byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        private static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now} - Starting {args[0]} - delete run.txt to terminate.");

            while (_fileInfo.Exists)
            {
                // save the previous internet state
                _internetStatePrev = _internetStateNow;

                // get the current internet state from the local function
                _internetStateNow = IsInternetUp() ? "up" : "down";

                // log any change of state
                if (!_internetStateNow.Equals(_internetStatePrev, 0))
                {
                    TimeSpan _interval = DateTime.Now - _dt;

                    // save the state change time
                    _dt = DateTime.Now;

                    Console.WriteLine($"{DateTime.Now} - Internet state is now <{_internetStateNow}>, " +
                        $"Previous state was <{_internetStatePrev}>, " +
                        $"Duration of previous state <{_interval.Minutes}> mins");
                }

                _fileInfo = new FileInfo(@".\run.txt");

                // log an alive message every hour
                DateTime dt = DateTime.Now;
                if (dt.Minute == 0)
                {
                    Console.WriteLine($"{DateTime.Now} - INFO - monitoring");
                }

                // sleep 30 seconds
                Thread.Sleep(30000);
            }

            Console.WriteLine($"{DateTime.Now} - Finished - press any key to terminate");
            Console.ReadLine();


            // local function to ping internet
            bool IsInternetUp()
            {
                Ping myPing = new Ping();
                //byte[] buffer = new byte[32];
                int timeout = 2000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(args[0], timeout, _buffer, pingOptions);

                myPing.Dispose();
                return reply.Status == IPStatus.Success;
            }

        }

    }
}
