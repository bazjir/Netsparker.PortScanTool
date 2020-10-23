using System;
using System.Collections.Generic;
using System.Text;

namespace Netsparker.PortScanTool.Helpers
{
    public interface IIPPortScanner
    {
        void StartScan(int taskCountToRunParallel, string ipStart, string ipEnd, Action<string> onOpenPortDetectedCallback, Action onScanOperationCompletedCallback, Action onScanAPortCompletedCallback, Action<Exception> onExceptionOccuredCallback);
        void TerminateScan();
        void AddTaskIfNewTaskCountIsMoreThanCurrent(int newParallelTaskCount, Action<string> onOpenPortDetectedCallback, Action onScanOperationCompletedCallback, Action onScanAPortCompletedCallback);
    }
}
