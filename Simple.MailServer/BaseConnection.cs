#region Header
// Copyright (c) 2013 Hans Wolff
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.Security.Cryptography;
using System.Reflection;


#endregion

using SMTPd.Mime;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using Mono.Security.X509;
using System.Threading;
using System.Threading.Tasks;
//using Mono.Security.Authenticode;
//using Mono.Security.Protocol.Tls;

namespace SMTPd
{
    [DebuggerDisplay("{RemoteEndPoint} -> {PortBinding}")]
    public abstract class BaseConnection : IClientConnection, ICanReadLineAsync
    {
		public NetworkStream NetworkStream { get; protected set; }
		public System.Net.Security.SslStream SSLStream { get; protected set; }
        public IPEndPoint RemoteEndPoint { get; protected set; }
        public PortListener PortBinding { get; protected set; }
        public TcpClient TcpClient { get; protected set; }
        public DateTime ConnectionInitiated { get; protected set; }
		public static bool UseSSL { get; protected set; }

		protected StringReaderStream Reader;
		protected StreamWriter Writer;
		protected StringReaderStream SSLReader;
		protected StreamWriter SSLWriter;

        public event EventHandler<RawLineEventArgs> RawLineReceived = (s, e) => { };
        public event EventHandler<RawLineEventArgs> RawLineSent = (s, e) => { };

        protected BaseConnection(PortListener portBinding, TcpClient tcpClient)
        {
            if (portBinding == null) throw new ArgumentNullException("portBinding");
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");

            PortBinding = portBinding;
            TcpClient = tcpClient;

            ConnectionInitiated = DateTime.UtcNow;


			/*var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadWrite);
			X509Certificate2 ca_cert = new X509Certificate2();
			ca_cert.Import (System.IO.File.ReadAllBytes ("/tmp/CA.p12"));
			store.Add(ca_cert);
			store.Close();

			System.Security.Cryptography.X509Certificates.X509Certificate2 CPrivate = new System.Security.Cryptography.X509Certificates.X509Certificate2();
			CPrivate.Import (System.IO.File.ReadAllBytes ("/tmp/ffwronbpi.feuerwehrcloud.de.p12"));
			System.Security.Cryptography.X509Certificates.X509Certificate CPublic = new System.Security.Cryptography.X509Certificates.X509Certificate2 ();
			CPublic.Import (System.IO.File.ReadAllBytes( "/tmp/ffwronbpi.feuerwehrcloud.de.cer"));

			try {
				SSLStream = new System.Net.Security.SslStream(tcpClient.GetStream(), true,  null, null);
				SSLStream.AuthenticateAsServer(CPrivate,false, System.Security.Authentication.SslProtocols.Default,true);
				//SSLStream.
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.Message);
			}

			Writer = new StreamWriter (SSLStream);
			Reader = new StringReaderStream (SSLStream, 4096);
			*/

            NetworkStream = tcpClient.GetStream();
            Reader = new StringReaderStream(NetworkStream);
            Writer = new StreamWriter(NetworkStream) { AutoFlush = true };
            RemoteEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
        }

        public abstract TimeSpan GetIdleTime();

        public async Task<byte[]> ReadLineAsync(CancellationToken cancellationToken)
        {
			byte[] rawLine;
			if (UseSSL) {
				rawLine = await SSLReader.ReadLineAsync(cancellationToken);
			} else {
				rawLine = await Reader.ReadLineAsync(cancellationToken);
			}
            RawLineReceived(this, new RawLineEventArgs(rawLine ?? new byte[0]));
            return rawLine;
        }

        public async Task WriteLineAsyncAndFireEvents(string line)
        {
			if (UseSSL) {
				RawLineSent(this, new RawLineEventArgs(SSLWriter.Encoding.GetBytes(line)));
				await SSLWriter.WriteLineAsync(line);
			} else {
				RawLineSent(this, new RawLineEventArgs(Writer.Encoding.GetBytes(line)));
                await TextWriter.Synchronized(Writer).WriteLineAsync(line);
			}
        }
			

		public virtual void Starttls()
		{


			var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadWrite);
			X509Certificate2 ca_cert = new X509Certificate2();
			ca_cert.Import (System.IO.File.ReadAllBytes ("/tmp/CA.p12"));
			store.Add(ca_cert);
			store.Close();

			System.Security.Cryptography.X509Certificates.X509Certificate2 CPrivate = new System.Security.Cryptography.X509Certificates.X509Certificate2();
			CPrivate.Import (System.IO.File.ReadAllBytes ("/tmp/ffwronbpi.feuerwehrcloud.de.p12"));
			System.Security.Cryptography.X509Certificates.X509Certificate CPublic = new System.Security.Cryptography.X509Certificates.X509Certificate2 ();
			CPublic.Import (System.IO.File.ReadAllBytes( "/tmp/ffwronbpi.feuerwehrcloud.de.cer"));

			try {
				SSLStream = new System.Net.Security.SslStream(this.NetworkStream, false,  null, null);
				SSLStream.AuthenticateAsServer(CPrivate,false, System.Security.Authentication.SslProtocols.Default,true);
				//SSLStream.
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.Message);
			}
			SSLWriter = new StreamWriter (SSLStream);
			SSLReader = new StringReaderStream (SSLStream, 4096);
			UseSSL = true;
		}
		public virtual void Disconnect()
        {
            try
            {
				if(UseSSL)
					SSLStream.Close();
				else
					NetworkStream.Close();

                TcpClient.Close();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // ignore
            }
        }

        #region Dispose
        private bool _disposed;
        private readonly object _disposeLock = new object();

        /// <summary>
        /// Inheritable dispose method
        /// </summary>
        /// <param name="disposing">true, suppress GC finalizer call</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    Disconnect();

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Free resources being used
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseConnection()
        {
            Dispose(false);
        }
        #endregion
    }
}
