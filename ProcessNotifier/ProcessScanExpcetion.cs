using System;
using System.Diagnostics;

namespace ProcessNotifier
{
    public class ProcessScanExpcetion
    {
        public readonly Exception Exception;
        public readonly Process Process;

        public ProcessScanExpcetion(Exception exception, Process process)
        {
            Exception = exception;
            Process = process;
        }
    }
}
