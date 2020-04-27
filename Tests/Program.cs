using System;
using ProcessNotifier;
using System.Diagnostics;

namespace Tests
{
    class Program
    {
        static bool ScannerStarted = false;
        static bool ScannerEnded = false;
        static bool ScannerException = false;
        static bool ScannerProcessException = false;
        static int Main(string[] args)
        {
            try
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
                Assert("Waiting for process scanner for about 20 seconds");
                AssertNot(processScanner.Wait(new TimeSpan(0, 0, 20)), "Scanner wait retunred correct value", "Scanner wait returnred incorrect value, scanner may stop running");
                Assert(Program.ScannerStarted, "Scanner start event was captured in the right moment", "Scanner start event was not raised");
                AssertNot(Program.ScannerProcessException, "All processes infomration were scanned correctly", "There were some error while scanning some of the proceses", true);
                AssertNot(Program.ScannerException, "Scanner is active without any exceptions", "An exception occured inside the scanner", true);
                processScanner.Stop();
                Assert("Stoping scanner, waiting for scanner to stop (timeout of 10 seconds)");
                Assert(processScanner.Wait(new TimeSpan(0, 0, 10)), "Scanner wait retunred correct value", "Scanner wait returnred incorrect value, scanner may stop running");
                Assert(Program.ScannerStarted, "Scanner start event was captured in the right moment", "Scanner start event was not raised");
                AssertNot(Program.ScannerProcessException, "All processes infomration were scanned correctly", "There were some error while scanning some of the proceses", true);
                AssertNot(Program.ScannerException, "Scanner is active without any exceptions", "An exception occured inside the scanner", true);
                Assert(Program.ScannerEnded, "Scanner end event was captured in the right moment", "Scanner end event was not raised");
            } 
            catch (Exception ex)
            {
                Console.WriteLine("");
                AssertFail($"Terminating tests, unable to continue with tests due to {ex.Message}");
                Console.ReadKey();
                return 1;
            }
            Console.ReadKey();
            return 0;
        }

        public static void OnExpcetion(object sender, Exception exception)
        {
            AssertFail($"[Event] Scanner exception, {exception.Message}");
            Program.ScannerException = true;
        }
        public static void OnProcessExpcetion(object sender, ProcessScanExpcetion processExcpetion)
        {
            AssertFail($"[Event] process scanner exception at process {processExcpetion.Process.ProcessName}, {processExcpetion.Exception.Message}");
            Program.ScannerProcessException = true;
        }
        public static void OnScannerStart(object sender, Scanner scanner)
        {
            Assert("[Event] Scanner started");
            Program.ScannerStarted = true;
        }
        public static void OnScannerStop(object sender, Scanner scanner)
        {
            Assert("[Event] Scanner stoped");
            Program.ScannerEnded = true;
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
