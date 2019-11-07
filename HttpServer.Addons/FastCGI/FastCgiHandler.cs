// -----------------------------------------------------------------------
// <copyright file="FastCgiHandler.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons.FastCgi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using HttpServer.Addons.FastCgi.Protocol;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FastCgiHandler
    {
        private const int FastCgiVersion = 1;
        /// <summary>
        /// Endpoint of the FastCGI application (i.e. ip and port where the 
        /// fcgi app, like PHP, listens).
        /// </summary>
        IPEndPoint endpoint;

        int timeout;
        bool keepAlive = false;

        MemoryStream stdout;
        MemoryStream stderr;

        public MemoryStream StandardOutput
        {
            get { return stdout; }
        }

        public MemoryStream StandardError
        {
            get { return stderr; }
        }

        public bool HasError
        {
            get { return (stderr != null && stderr.Length > 0); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FastCgiHandler(int port, int timeout)
        {
            this.endpoint = new IPEndPoint(IPAddress.Loopback, port);
            this.timeout = timeout;

            this.keepAlive = false;
        }

        public int ProcessRequest(int id, IRequest request, Dictionary<string, string> env)
        {
            Socket socket = null;
            string response = String.Empty;

            try
            {
                // Connect to fcgi app
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = timeout;
                socket.Connect(endpoint);

                lock (socket)
                {
                    using (var stream = new NetworkStream(socket))
                    {
                        SendBeginRequest(id, stream);
                        SendParams(id, stream, env);
                        SendInput(id, stream, request.Body);

                        var endRequestBody = ReadOutput(id, stream);

                        stdout.Position = 0;

                        if (stderr != null)
                        {
                            stderr.Position = 0;
                        }

                        return endRequestBody.AppStatus;
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: handle exceptions
            }
            finally
            {
                if (socket != null)
                {
                    if (socket.Connected) socket.Close();
                }
            }

            return 666;
        }

        private void SendBeginRequest(int id, NetworkStream ns)
        {
            var beginRequestBody = new BeginRequestBody()
            {
                Role = Role.Responder,
                KeepConnection = keepAlive
            };

            byte[] bytes = beginRequestBody.ToByteArray();

            this.SendRecord(id, ns, RecordType.BeginRequest, bytes, 0, bytes.Length);
        }

        private void SendParams(int id, NetworkStream ns, Dictionary<string, string> env)
        {
            if (env == null)
            {
                this.SendRecord(id, ns, RecordType.Params, null, 0, 0);
            }
            else
            {
                MemoryStream stream = new MemoryStream();

                foreach (var param in env)
                {
                    if (String.IsNullOrEmpty(param.Value))
                    {
                        continue;
                    }

                    NameValuePair paramBody = new NameValuePair()
                    {
                        Name = param.Key,
                        Value = param.Value
                    };

                    stream.Write(paramBody.GetHeader(), 0, 8);
                    stream.Write(Encoding.ASCII.GetBytes(param.Key), 0, param.Key.Length);
                    stream.Write(Encoding.ASCII.GetBytes(param.Value), 0, param.Value.Length);
                }

                byte[] bytes = stream.ToArray();
                this.SendRecord(id, ns, RecordType.Params, bytes, 0, bytes.Length);
                this.SendRecord(id, ns, RecordType.Params);
            }
        }

        private void SendInput(int id, NetworkStream ns, Stream input)
        {
            if (input != null && input.Length > 0)
            {
                byte[] buffer = new byte[1024];
                int count;
                while ((count = input.Read(buffer, 0, 1024)) > 0)
                {
                    this.SendRecord(id, ns, RecordType.StandardInput, buffer, 0, count);
                }
            }
            this.SendRecord(id, ns, RecordType.StandardInput);
        }

        private EndRequestBody ReadOutput(int id, NetworkStream ns)
        {
            stdout = new MemoryStream();

            Record record;
            while (true)
            {
                record = ReadRecord(ns);

                if (record.RequestId != id)
                {
                    // TODO: error
                }

                if (record.Type == RecordType.StandardOutput)
                {
                    stdout.Write(record.ContentData, 0, record.ContentLength);
                }
                else if (record.Type == RecordType.StandardError)
                {
                    if (stderr == null)
                    {
                        stderr = new MemoryStream();
                    }

                    stderr.Write(record.ContentData, 0, record.ContentLength);
                }
                else if (record.Type == RecordType.EndRequest)
                {
                    return new EndRequestBody(record.ContentData);
                }
            }
        }

        /// <summary>
        /// Sends a record with no data to the FastCGI child process.
        /// </summary>
        private void SendRecord(int id, NetworkStream ns, RecordType recordType)
        {
            this.SendRecord(id, ns, recordType, null, 0, 0);
        }

        /// <summary>
        /// Sends a record to the FastCGI child process.
        /// </summary>
        private void SendRecord(int id, NetworkStream ns, RecordType recordType, byte[] buffer, int offset, int count)
        {
            if (buffer != null)
            {
                if (offset < 0 || count < 0 || offset + count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            Record record = new Record();
            record.ContentLength = count;
            record.PaddingLength = 0;
            record.RequestId = id;
            record.Type = recordType;
            record.Version = FastCgiVersion;

            MemoryStream data = new MemoryStream();
            data.Write(record.GetHeader(), 0, 8);
            if (buffer != null)
            {
                data.Write(buffer, offset, count);
            }

            ns.Write(data.ToArray(), 0, (int)data.Length);
        }


        /// <summary>
        /// Reads a record from the FastCGI child process.
        /// </summary>
        private Record ReadRecord(NetworkStream ns)
        {
            Record record = new Record(this.ReadData(ns, 8));
            if (record.ContentLength > 0)
            {
                record.ContentData = this.ReadData(ns, record.ContentLength);
            }
            if (record.PaddingLength > 0)
            {
                record.PaddingData = this.ReadData(ns, record.PaddingLength);
            }
            return record;
        }

        private byte[] ReadData(NetworkStream ns, int length)
        {
            byte[] buffer = new byte[length];

            int offset = 0;

            while (offset < length)
            {
                buffer[offset++] = (byte)ns.ReadByte();
            }

            //int count = ns.Read(buffer, 0, length);

            return buffer;
        }
    }
}
