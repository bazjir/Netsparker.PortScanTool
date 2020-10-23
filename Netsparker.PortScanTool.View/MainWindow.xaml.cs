using Microsoft.Extensions.Logging;
using Netsparker.PortScanTool.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Netsparker.PortScanTool.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PortScanToolViewModel _portScanToolViewModel { get; set; }
        private ILogger _logger;

        public MainWindow(PortScanToolViewModel portScanToolViewModel, ILogger<Netsparker.PortScanTool.View.MainWindow> logger)
        {
            InitializeComponent();
            _portScanToolViewModel = portScanToolViewModel;
            DataContext = _portScanToolViewModel;
            _portScanToolViewModel.ExceptionEventHandler += (sender, args) => { OnExceptionOccuredOnTaskDuringScanning(sender, args); };
            _logger = logger;
            _portScanToolViewModel.OpenPortDetectedEventHandler += (sender, detectedPortWithIp) => AddOpenPortToListBox(sender, detectedPortWithIp);
        }

        private void AddOpenPortToListBox(object sender,string detectedPortWithIp)
        {
            try
            {
                Dispatcher.Invoke(new Action(() => this._portScanToolViewModel.ListBxDetectedOpenPorts.Add(detectedPortWithIp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BtnStartScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _portScanToolViewModel.StartScan();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEndScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _portScanToolViewModel.EndScan();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSetSampleIPRange_Click(object sender, RoutedEventArgs e)
        {
            _portScanToolViewModel.SetSampleIPRange();
        }

        private void OnExceptionOccuredOnTaskDuringScanning(object sender, ErrorEventArgs eventArgs)
        {
            _logger.LogError(eventArgs.GetException().Message);
            MessageBox.Show(eventArgs.GetException().Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
