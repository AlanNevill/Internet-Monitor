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

        private static FileInfo _fileInfo = new FileInfo(@"./run.txt");
        private static readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        private static readonly StreamWriter _writer = File.AppendText(@"./logfile.txt");
        private static string _mess;

        private static void Main(string[] args)
        {
            if (args.Length==0)
            {
                args[0] = "1.1.1.1";
            }

            _writer.AutoFlush = true;

            _mess = $"{DateTime.Now} - Starting {args[0]} - delete run.txt to terminate.";
            Console.WriteLine(_mess);
            Log(_mess);

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

                    _mess = $"{DateTime.Now}, Internet state is now, {_internetStateNow}, Duration of previous state, {_internetStatePrev}, {_interval.TotalMinutes}, mins";
                    Console.WriteLine(_mess);
                    Log(_mess);
                }

                _fileInfo = new FileInfo(@"./run.txt");

                // log an alive message every hour
                DateTime dt = DateTime.Now;
                if (dt.Minute == 0)
                {
                    Console.WriteLine($"{DateTime.Now} - INFO - monitoring");
                }

                // sleep 31 seconds
                Thread.Sleep(31000);
            }

            _mess = $"{DateTime.Now} - Finished - no run.txt file - press any key to terminate";
            Console.WriteLine(_mess);
            Log(_mess);

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

        private static void Log(string mess)
        {
            _writer.WriteLine(mess);
        }
    }
}
