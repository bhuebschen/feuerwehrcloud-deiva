
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A FastCGI end request record.
    /// </summary>
    internal struct EndRequestBody
    {
        /// <summary>
        /// Initialises a new instance of <see cref="EndRequestBody" /> from the record body.
        /// </summary>
        /// <param name="Data">Record body as a byte array.</param>
        /// <exception cref="ArgumentException">Data length is not 8 bytes.</exception>
        /// <exception cref="ArgumentNullException">Data is null.</exception>
        public EndRequestBody(byte[] Data)
            : this()
        {
            this.AppStatus =
                (Data[0] << 24) +
                (Data[1] << 16) +
                (Data[2] << 8) +
                Data[3];
            this.ProtocolStatus = (ProtocolStatus)Data[4];
        }

        /// <summary>
        /// Gets the role-specific app status.
        /// </summary>
        public int AppStatus;

        /// <summary>
        /// Gets the a protocol-level status code.
        /// </summary>
        public ProtocolStatus ProtocolStatus;
    }
}
