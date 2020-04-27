using System;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace ProcessNotifier
{
    public class Scanner:IDisposable
    {
        public static readonly TimeSpan DefaultScanInterval = new TimeSpan(0, 0, 5);
        
        public readonly TimeSpan ScanInterval;
        private Hashtable processHistory;
        private EventWaitHandle processWaitHandle;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken cancelToken;
        private Thread scannerThread;
        private object SyncLock;
        private bool raiseEvents;

        public event EventHandler<Scanner> ScannerStart;
        public event EventHandler<Scanner> ScannerEnd;
        public event EventHandler<Scanner> ScanBegin;
        public event EventHandler<Scanner> ScanEnd;
        public event EventHandler<Process> ProcessCreation;
        public event EventHandler<Exception> OnException;
        public event EventHandler<ProcessScanExpcetion> OnProcessExpcetion;

        public Scanner(TimeSpan scanInterval )
        {
            this.ScanInterval = scanInterval;
            this.processHistory = new Hashtable();
            this.SyncLock = new object();
            this.processWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }
        public Scanner() : this(Scanner.DefaultScanInterval){}


        public void Start(bool raiseEvents = true)
        {
            this.Stop();
            this.raiseEvents = raiseEvents;
            this.cancelTokenSource = new CancellationTokenSource();
            this.cancelToken = this.cancelTokenSource.Token;
            this.scannerThread = new Thread(this.innerLoop);
            this.scannerThread.IsBackground = true;
            this.scannerThread.Start();
        }
        public void Stop()
        {
            if (this.cancelTokenSource != null) this.cancelTokenSource.Cancel();
            this.processWaitHandle.Set();
            if (this.scannerThread != null) this.scannerThread.Join();
            this.processWaitHandle.Reset();
        }
        public void Wait()
        {
            if (this.scannerThread != null) this.scannerThread.Join();
        }
        public bool Wait(TimeSpan timeout)
        {
            if (this.scannerThread != null) return this.scannerThread.Join(timeout);
            return false;
        }
        public void ScanNow(bool raiseEvents = true)
        {
            if (!this.scannerThread.IsAlive) throw new Exception("Scanner wasn't started, please start the scanner before calling the ScanNow method");
            this.raiseEvents = raiseEvents;
            this.processWaitHandle.Set();
        }
        public void ClearHistory(bool reScan = false)
        {
            lock(this.SyncLock)
            {
                this.processHistory.Clear();
            }
            if (reScan) this.ScanNow();
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void innerLoop()
        {
            DateTime startScanning;
            TimeSpan scanDuration;

            this.ScannerStart?.Invoke(this, this);
            do
            {
                startScanning = DateTime.Now;
                this.innerRunScan(this.raiseEvents);
                scanDuration = DateTime.Now - startScanning;
                this.raiseEvents = true;
                if(scanDuration < this.ScanInterval)
                    this.processWaitHandle.WaitOne(this.ScanInterval - scanDuration);
            } while (!this.cancelToken.IsCancellationRequested);
            this.ScannerEnd?.Invoke(this, this);
        }
        private void innerRunScan(bool raiseEvents)
        {
            Process[] allProcesses = Process.GetProcesses();

            this.ScanBegin?.Invoke(this, this);
            lock (this.SyncLock)
            {
                foreach (Process singleProcess in allProcesses)
                {
                    if (this.TryInsertProcessToHistoryUnsafe(singleProcess) && this.raiseEvents)
                    {
                        this.ProcessCreation?.Invoke(this, singleProcess);   
                    }
                }
            }
            this.ScanEnd?.Invoke(this, this);
        }
        private bool TryInsertProcessToHistoryUnsafe(Process process)
        {
            try
            {
                if (process.Id == 0) return false; // Fix for idle process in windows
                if (process.HasExited) return false; // Fix for errors about processes that already exited 
                if (this.processHistory.ContainsKey(process.Id))
                {
                    if (((DateTime)this.processHistory[process.Id]) == process.StartTime)
                    {
                        return false;
                    }
                    else
                    {

                        this.processHistory[process.Id] = process.StartTime;
                        return true;
                    }
                }
                else
                {
                    this.processHistory.Add(process.Id, process.StartTime);
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.OnProcessExpcetion?.Invoke(this, new ProcessScanExpcetion(ex, process));
            }
            return false;
                
            
        }
    }
}
