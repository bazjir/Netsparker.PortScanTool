using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Netsparker.PortScanTool.Helpers
{
    /// <summary>
    /// A class contains methods to manage IP data from the given range.
    /// </summary>
    public class IPDataProvider : IIPDataProvider
    {
        private readonly Queue<string> IPAddressesWaitingForScanning;
        private static readonly object _synchObject = new object();

        public IPDataProvider()
        {
            // Initialize queue.
            IPAddressesWaitingForScanning = new Queue<string>();
        }

        /// <summary>
        /// Build IP list from the given IP range.
        /// </summary>
        /// <param name="from">Start IP</param>
        /// <param name="to">End IP</param>
        public void BuildIPListFromIpRange(string from, string to, CancellationToken ct)
        {
            try
            {
                int counter;

                int[] ipStart = Array.ConvertAll(from.Split('.'), int.Parse);
                int[] ipEnd = Array.ConvertAll(to.Split('.'), int.Parse);

                int startIP = (
                   ipStart[0] << 24 |
                   ipStart[1] << 16 |
                   ipStart[2] << 8 |
                   ipStart[3]);

                int endIP = (
                   ipEnd[0] << 24 |
                   ipEnd[1] << 16 |
                   ipEnd[2] << 8 |
                   ipEnd[3]);

                for (counter = startIP; counter <= endIP; counter++)
                {
                    // if this token has had cancellation.
                    // Clear the queue and break loop.
                    if (ct.IsCancellationRequested)
                    {
                        IPAddressesWaitingForScanning.Clear();
                        break;
                    }

                    string ipAddress = $"{(counter & 0xFF000000) >> 24}.{(counter & 0x00FF0000) >> 16}.{(counter & 0x0000FF00) >> 8}.{counter & 0x000000FF}";
                    IPAddressesWaitingForScanning.Enqueue(ipAddress);
                }
            }
            catch
            {
                throw new ArgumentException("Please enter the ip address in correct format. ex: 45.151.250.150");
            }
        }

        /// <summary>
        /// Return an ip address to scan and dequeque the returning ip address from waiting queue ,and enqueue it to scanning queue.
        /// </summary>
        /// <returns></returns>
        public string GetIPAddressToScan()
        {
            // Synchronize access to the shared member.
            lock (_synchObject)
            {
                return IPAddressesWaitingForScanning.Any() ? IPAddressesWaitingForScanning.Dequeue() : null;
            }
        }

        public bool AnyIpAddressWaitingForScanning => IPAddressesWaitingForScanning.Any();
    }
}
