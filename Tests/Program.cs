using System;
using ProcessNotifier;
using System.Diagnostics;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Assert("Starting tests");
            Scanner processScanner = Assert(new Scanner(Scanner.DefaultScanInterval), "Creating scanner object", "Unable to create scan object");
            processScanner.ProcessCreation += OnNewProcess;
            processScanner.ScanBegin += OnScanBegin;
            processScanner.ScanEnd += OnScanEnd;
            processScanner.ScannerEnd += OnScannerStop;
            processScanner.ScannerStart += OnScannerStart;
            processScanner.OnException += OnExpcetion;
            processScanner.OnProcessExpcetion += OnProcessExpcetion;
            Assert("Subscribe to scanner events");

            processScanner.Start();
            Assert("Scanner was triggered to start");

            Console.ReadKey();
        }

        public static void OnExpcetion(object sender, Exception exception)
        {
            AssertFail($"[Event] Scanner exception, {exception.Message}");
        }
        public static void OnProcessExpcetion(object sender, ProcessScanExpcetion processExcpetion)
        {
            AssertFail($"[Event] process scanner exception at process {processExcpetion.Process.ProcessName}, {processExcpetion.Exception.Message}");
        }
        public static void OnScannerStart(object sender, Scanner scanner)
        {
            Assert("[Event] Scanner started");
        }
        public static void OnScannerStop(object sender, Scanner scanner)
        {
            Assert("[Event] Scanner stoped");
        }
        public static void OnScanBegin(object sender, Scanner scanner)
        {
            Assert("[Event] scanner is starting a scan");
        }
        public static void OnScanEnd(object sender, Scanner scanner)
        {
            Assert("[Event] scanner is ending a scan");
        }
        public static void OnNewProcess(object sender, Process process)
        {
            Assert($"[Event] new process: {process.ProcessName}");
        }

        public static void AssertBed(string bedMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[FAIL] ");
            Console.ResetColor();
            Console.WriteLine(bedMessage);
        }
        public static void AssertGood(string goodMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[ OK ] ");
            Console.ResetColor();
            Console.WriteLine(goodMessage);
        }
        public static T AssertNot<T>(T status, string goodMessage, string bedMessage = null, bool throwException = false)
        {
            if (status.Equals(default(T)))
            {
                if (goodMessage != null) Program.AssertGood(goodMessage);
            }
            else
            {
                if (bedMessage != null) Program.AssertBed(bedMessage);
                if (throwException) throw new Exception(bedMessage);
            }
            return status;
        }
        public static T Assert<T>(T status, string goodMessage, string bedMessage=null, bool throwException=false)
        {
            if(status.Equals(default(T)))
            {
                if(bedMessage != null)  Program.AssertBed(bedMessage);
                if(throwException)throw new Exception(bedMessage);
            }
            else
            {
                if (goodMessage != null) Program.AssertGood(goodMessage);
            }
            return status;
        }
        public static void Assert(string goodMessage) => Program.Assert(true, goodMessage);
        public static void AssertFail(string bedMessage) => Program.Assert(false, null, bedMessage);
    }
}
