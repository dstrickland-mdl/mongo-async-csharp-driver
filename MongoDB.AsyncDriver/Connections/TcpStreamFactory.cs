﻿/* Copyright 2013-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a factory for a binary stream over a TCP/IP connection.
    /// </summary>
    public class TcpStreamFactory : IStreamFactory
    {
        // fields
        private readonly TcpStreamSettings _settings;

        // constructors
        public TcpStreamFactory()
        {
            _settings = new TcpStreamSettings();
        }

        public TcpStreamFactory(TcpStreamSettings settings)
        {
            _settings = settings;
        }

        // public methods
        public async Task<Stream> CreateStreamAsync(DnsEndPoint endPoint, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            var ipEndPoint = await ResolveAsync(endPoint, slidingTimeout, cancellationToken);
            var protocolType = (ipEndPoint.AddressFamily == AddressFamily.InterNetworkV6) ? ProtocolType.IPv6 : ProtocolType.IP;
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, protocolType);
            await ConnectAsync(socket, ipEndPoint, slidingTimeout, cancellationToken);
            socket.NoDelay = true;
            socket.ReceiveBufferSize = _settings.ReceiveBufferSize;
            socket.SendBufferSize = _settings.SendBufferSize;

            var stream = new NetworkStream(socket, ownsSocket: true);
            var readTimeout = (int)_settings.ReadTimeout.TotalMilliseconds;
            if (readTimeout != 0)
            {
                stream.ReadTimeout = readTimeout;
            }
            var writeTimeout = (int)_settings.WriteTimeout.TotalMilliseconds;
            if (writeTimeout != 0)
            {
                stream.WriteTimeout = writeTimeout;
            }

            return stream;
        }

        // non-public methods
        private Task ConnectAsync(Socket socket, IPEndPoint ipEndPoint, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // TODO: handle timeout and cancellationToken
            return Task.Factory.FromAsync(socket.BeginConnect(ipEndPoint, null, null), socket.EndConnect);
        }

        private async Task<IPEndPoint> ResolveAsync(DnsEndPoint dnsEndPoint, TimeSpan timeout, CancellationToken cancellationToken)
        {
            string message;

            AddressFamily desiredAddressFamily;
            switch (dnsEndPoint.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                case AddressFamily.Unspecified:
                case AddressFamily.Unknown:
                    desiredAddressFamily = AddressFamily.InterNetwork;
                    break;

                case AddressFamily.InterNetworkV6:
                    desiredAddressFamily = AddressFamily.InterNetworkV6;
                    break;

                default:
                    message = string.Format("AddressFamily '{0}' is not supported.", dnsEndPoint.AddressFamily);
                    throw new ArgumentException(message, "dnsEndPoint");
            }

            var ipAddresses = await Dns.GetHostAddressesAsync(dnsEndPoint.Host);
            foreach (var ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == desiredAddressFamily)
                {
                    return new IPEndPoint(ipAddress, dnsEndPoint.Port);
                }
            }

            message = string.Format("Unable to resolve host name '{0}' for address family '{1}.", dnsEndPoint.Host, desiredAddressFamily);
            throw new SocketException((int)SocketError.HostNotFound);
        }
    }
}
