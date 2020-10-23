using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Netsparker.PortScanTool.Helpers
{
    /// <summary>
    /// The purpose of this class is to perform TCP requests to all IPs in the given range in parallel.
    /// And give information via callbacks to the ViewModel to visualize these information on the UI.
    /// Moreover, it create as much as tasks requested and manages these tasks by creating new ones and cancelling via CancellationTokenSource.
    /// 
    /// </summary>
    public class IPPortScanner : IIPPortScanner
    {
        private readonly IIPDataProvider _ipDataProvider;
        private List<Task> _tasks;
        private bool _isScannerRunning = false;
        private CancellationTokenSource _cancellationTokenSource;

        public IPPortScanner(IIPDataProvider dataProvider)
        {
            _ipDataProvider = dataProvider;
        }

        public async void StartScan(int taskCountToRunParallel, string ipStart, string ipEnd, Action<string> onOpenPortDetectedCallback, Action onScanOperationCompletedCallback, Action onScanAPortCompletedCallback, Action<Exception> onExceptionOccuredCallback)
        {
            _tasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Create a task to generate ip list from the given range.
                Task buildIPListFromRangeTask = Task.Factory.StartNew(() => _ipDataProvider.BuildIPListFromIpRange(ipStart, ipEnd, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
                _tasks.Add(buildIPListFromRangeTask);
                _isScannerRunning = true;

                // Create tasks as requested.
                for (int i = 0; i < taskCountToRunParallel; i++)
                {
                    _tasks.Add(Task.Factory.StartNew(() => DoScan(onOpenPortDetectedCallback, onScanAPortCompletedCallback, _cancellationTokenSource.Token), _cancellationTokenSource.Token));
                }

                await Task.WhenAll(_tasks).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                onExceptionOccuredCallback(ex);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _isScannerRunning = false;
                onScanOperationCompletedCallback();
            }
        }

        /// <summary>
        /// Do scanning on IP Ports.
        /// </summary>
        /// <param name="uiCallbackToShowOpenPort"></param>
        /// <param name="scanPortCompletedCallback"></param>
        /// <param name="ct"></param>
        private void DoScan(Action<string> uiCallbackToShowOpenPort, Action scanPortCompletedCallback, CancellationToken ct)
        {
            while (_tasks[0].Status == TaskStatus.Running || _ipDataProvider.AnyIpAddressWaitingForScanning)
            {
                // If the cancellation is requested, terminate the operation.
                if (ct.IsCancellationRequested) break;

                string ipAddressToScan = _ipDataProvider.GetIPAddressToScan();
                if (ipAddressToScan != null)
                {
                    // Scan all ports in parallel.
                    Parallel.For(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, (port, parallelLoopState) =>
                     {
                         // If the cancellation is requested, terminate the operation.
                         if (ct.IsCancellationRequested) parallelLoopState.Break();

                         using (var client = new TcpClient() { SendTimeout = 3000 })
                         {
                             try
                             {

                                 client.Connect(IPAddress.Parse(ipAddressToScan), port);
                                 // Return result.
                                 uiCallbackToShowOpenPort($"{ipAddressToScan}:{port}");
                             }
                             catch (Exception ex)
                             {
                                 if (!ct.IsCancellationRequested)
                                 {
                                     scanPortCompletedCallback();
                                 }
                             }
                         }
                     });
                }
                else
                {
                    // Give some time to ip build task to queue new IPs.
                    Thread.Sleep(300);
                }
            }
        }

        /// <summary>
        /// Add new task if the scanner is running and current tast count is less than the requested.
        /// </summary>
        /// <param name="newParallelTaskCount"></param>
        /// <param name="uiCallbackToShowOpenPort"></param>
        /// <param name="scanOperationCompletedCallBack"></param>
        /// <param name="scanPortCompletedCallback"></param>
        public void AddTaskIfNewTaskCountIsMoreThanCurrent(int newParallelTaskCount, Action<string> uiCallbackToShowOpenPort, Action scanOperationCompletedCallBack, Action scanPortCompletedCallback)
        {
            if (_isScannerRunning && newParallelTaskCount > (_tasks.Count - 1))
            {
                _tasks.Add(Task.Factory.StartNew(() => DoScan(uiCallbackToShowOpenPort, scanPortCompletedCallback, _cancellationTokenSource.Token)));
            }
        }

        /// <summary>
        /// Cancel the all created tasks.
        /// </summary>
        public void TerminateScan()
        {
            _cancellationTokenSource?.Cancel();
            
            _isScannerRunning = false;
        }
    }
}
