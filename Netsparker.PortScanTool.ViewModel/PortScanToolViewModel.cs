using Netsparker.PortScanTool.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace Netsparker.PortScanTool.ViewModel
{
    /// <summary>
    /// PortScanToolViewModel contains all properties and actions to 
    /// perform a port scan operation on the given ip range.
    /// </summary>
    public class PortScanToolViewModel : INotifyPropertyChanged
    {
        private readonly IIPPortScanner _portScanner;
        public ErrorEventHandler ExceptionEventHandler;
        public EventHandler<string> OpenPortDetectedEventHandler;
        private static readonly object _synchObject = new object();

        public PortScanToolViewModel(IIPPortScanner portScanner)
        {
            _portScanner = portScanner;
        }

        public PortScanToolViewModel()
        {

        }

        #region PortScanToolForm Properties.
        private string ipAddressStart;
        public string IPAddressStart
        {
            get => ipAddressStart;
            set
            {
                if (ipAddressStart != value)
                {
                    ipAddressStart = value;
                    RaisePropertyChanged(nameof(IPAddressStart));
                }
            }
        }

        private string ipAddressEnd;
        public string IPAddressEnd
        {
            get => ipAddressEnd;
            set
            {
                if (ipAddressEnd != value)
                {
                    ipAddressEnd = value;
                    RaisePropertyChanged(nameof(IPAddressEnd));
                }
            }
        }

        private bool isBtnEndScanVisible;
        public bool IsBtnEndScanVisible
        {
            get { return isBtnEndScanVisible; }
            set
            {
                if (isBtnEndScanVisible == value)
                    return;
                isBtnEndScanVisible = value;
                RaisePropertyChanged(nameof(IsBtnEndScanVisible));
            }
        }

        private int parallelTaskCount = 1;
        public int ParallelTaskCount
        {
            get { return parallelTaskCount; }
            set
            {
                if (parallelTaskCount == value)
                    return;
                parallelTaskCount = value;
                RaisePropertyChanged(nameof(ParallelTaskCount));
                _portScanner.AddTaskIfNewTaskCountIsMoreThanCurrent(value, OnOpenPortDetected, OnScanOperationCompleted, OnScanAPortCompleted);
            }
        }

        private int scannedPortCount;

        public int ScannedPortCount
        {
            get { return scannedPortCount; }
            set
            {
                scannedPortCount = value;
                RaisePropertyChanged(nameof(ScannedPortCount));
            }
        }

        private ObservableCollection<string> listBxDetectedOpenPorts = new ObservableCollection<string>();
        public ObservableCollection<string> ListBxDetectedOpenPorts
        {
            get { return listBxDetectedOpenPorts; }
            set
            {
                if (listBxDetectedOpenPorts == value)
                    return;

                listBxDetectedOpenPorts = value;
                RaisePropertyChanged(nameof(ListBxDetectedOpenPorts));
            }
        }

        private bool isLblScanOperationInfoVisible;

        public bool IsLblScanOperationInfoVisible
        {
            get { return isLblScanOperationInfoVisible; }
            set
            {
                isLblScanOperationInfoVisible = value;
                RaisePropertyChanged(nameof(IsLblScanOperationInfoVisible));
            }
        }

        private string lblScanOperationInfoText;

        public string LblScanOperationInfoText
        {
            get { return lblScanOperationInfoText; }
            set
            {
                lblScanOperationInfoText = value;
                RaisePropertyChanged(nameof(LblScanOperationInfoText));
            }
        }

        private bool btnStartScanEnabled = true;
        public bool BtnStartScanEnabled
        {
            get { return btnStartScanEnabled; }
            set
            {
                btnStartScanEnabled = value;
                RaisePropertyChanged(nameof(BtnStartScanEnabled));
            }
        }

        private bool btnEndScanEnabled;
        public bool BtnEndScanEnabled
        {
            get { return btnEndScanEnabled; }
            set
            {
                btnEndScanEnabled = value;
                RaisePropertyChanged(nameof(BtnEndScanEnabled));
            }
        }
        #endregion

        #region Action methods on form.

        /// <summary>
        /// Starts port scan operation asynchronously by calling IPPortScanner.StartScan(..) async method 
        /// </summary>
        public void StartScan()
        {
            // Validate IPs.
            if (!IPAddress.TryParse(IPAddressStart, out _) || !IPAddress.TryParse(IPAddressStart, out _))
            {
                throw new ArgumentException("Please enter a valid IP range. ex: from 45.151.250.150 to 45.151.250.250");
            }

            ClearListBxDetectedOpenPorts();
            ScannedPortCount = 0;
            LblScanOperationInfoText = "Scan operation's been started, please wait...";
            IsLblScanOperationInfoVisible = true;
            BtnStartScanEnabled = false;
            IsBtnEndScanVisible = true;
            BtnEndScanEnabled = true;

            _portScanner.StartScan(ParallelTaskCount, IPAddressStart, IPAddressEnd, OnOpenPortDetected, OnScanOperationCompleted, OnScanAPortCompleted, OnExceptionOccuredDuringScanning);
        }

        /// <summary>
        /// Terminate scan operation.
        /// </summary>
        public void EndScan()
        {
            LblScanOperationInfoText = "Scan operation termination's been started, please wait...";
            BtnEndScanEnabled = false;

            _portScanner.TerminateScan();
        }

        public void SetSampleIPRange()
        {
            IPAddressStart = "45.151.250.150";
            IPAddressEnd = "45.151.250.250";
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Callbacks.
        /// <summary>
        /// An open port's detected, add it to list which holds
        /// the found open ports.
        /// </summary>
        /// <param name="detectedOpenPortWithIP"></param>
        private void OnOpenPortDetected(string detectedOpenPortWithIP)
        {
            // TODO: ListBxDetectedOpenPorts.Insert(0, detectedOpenPortWithIP) command doesn't trigger update
            // on the UI. Because of limited-time, the following fast workaround has been done: inserting new item to list 
            // has been done by Dispather on the view.
            //ListBxDetectedOpenPorts.Insert(0, detectedOpenPortWithIP);
            OpenPortDetectedEventHandler.Invoke(this, detectedOpenPortWithIP);
        }

        /// <summary>
        /// A port scanning completed, increment ScannedPortCount, the 
        /// new value will be displayed on the UI automatically.
        /// </summary>
        private void OnScanAPortCompleted()
        {
            // Synchronize access to the shared member.
            lock (_synchObject)
            {
                ScannedPortCount++;
            }
        }

        /// <summary>
        /// Scan operation completed. 
        /// </summary>
        private void OnScanOperationCompleted()
        {
            BtnStartScanEnabled = true;
            IsBtnEndScanVisible = false;
            LblScanOperationInfoText = "Scan operation's completed";
        }

        private void OnExceptionOccuredDuringScanning(Exception ex)
        {
            ExceptionEventHandler?.Invoke(this, new ErrorEventArgs(ex));
        }
        #endregion

        #region Helper Methods.
        public void ClearListBxDetectedOpenPorts()
        {
            ListBxDetectedOpenPorts.Clear();
        }
        #endregion

    }
}
