using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;

namespace InternetMonitor
{
    internal class Program
    {
        private static string _internetStateNow = "monitor starting";
        private static string _internetStatePrev;
        private static DateTime _dt = DateTime.Now;

        private static FileInfo _fileInfo = new FileInfo(@"./run.txt");
        private static readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        private static readonly StreamWriter _writer = File.AppendText(@"./logfile.txt");
        private static string _mess;

        // Uses System.CommandLine beta library
        // see https://github.com/dotnet/command-line-api/wiki/Your-first-app-with-System.CommandLine

        public static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("Internet monitor")
            {
                new Option("--host", "Host ip address or name such as 8.8.8.8, www.google.com")
                    {
                        Argument = new Argument<string>(),
                        Required = true
                    }
            };

            rootCommand.TreatUnmatchedTokensAsErrors = true;

            rootCommand.Handler = CommandHandler.Create((string host) =>
            {
                Monitor(host);
            });

            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Monitor(string host)
        {
            // default is not to flush
            _writer.AutoFlush = true;

            // startup message
            _mess = $"{DateTime.Now} - v{Assembly.GetExecutingAssembly().GetName().Version}. Starting with host: {host}. Delete run.txt to terminate.";
            Console.WriteLine(_mess);
            Log(_mess);

            // main loop
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

                // log an alive message once every hour
                DateTime dt = DateTime.Now;
                if (dt.Minute == 0)
                {
                    Console.WriteLine($"{DateTime.Now} - INFO - monitoring");
                }

                // sleep 31 seconds
                Thread.Sleep(31000);
            }

            // finish message
            _mess = $"{DateTime.Now} - Finished - no run.txt file - press any key to terminate";
            Console.WriteLine(_mess);
            Log(_mess);

            Console.ReadLine();


            // local function to ping internet which uses args[]
            bool IsInternetUp()
            {
                int timeout = 2000;
                PingOptions pingOptions = new PingOptions();
                using (Ping myPing = new Ping())
                {
                    try
                    {
                        PingReply reply = myPing.Send(host, timeout, _buffer, pingOptions);
                        return reply.Status == IPStatus.Success;
                    }
                    catch (Exception ex)
                    {
                        _mess = $"{DateTime.Now} - ERROR - {ex.ToString()}";
                        Console.WriteLine(_mess);
                        Log(_mess);
                        return true;
                    }
                }
            }
        }

        private static void Log(string mess)
        {
            _writer.WriteLine(mess);
        }
    }
}
