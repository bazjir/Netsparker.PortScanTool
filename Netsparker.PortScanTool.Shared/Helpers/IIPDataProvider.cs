using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Netsparker.PortScanTool.Helpers
{
    public interface IIPDataProvider
    {
        void BuildIPListFromIpRange(string from, string to, CancellationToken ct);
        string GetIPAddressToScan();
        bool AnyIpAddressWaitingForScanning { get; }
    }
}
