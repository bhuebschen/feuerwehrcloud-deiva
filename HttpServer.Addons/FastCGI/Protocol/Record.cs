
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Describes a FastCGI record
    /// </summary>
    internal struct Record
    {
        /// <summary>
        /// Initialises a new instance of <see cref="Record" /> from header data.
        /// </summary>
        /// <param name="header">Header as a byte array.</param>
        /// <exception cref="ArgumentException">Header length is not 8 bytes.</exception>
        /// <exception cref="ArgumentNullException">Header is null.</exception>
        public Record(byte[] header)
            : this()
        {
            if (header == null)
            {
                throw new ArgumentNullException("Header");
            }
            if (header.Length != 8)
            {
                throw new ArgumentException("Header length is not 8");
            }

            this.Version = header[0];
            this.Type = (RecordType)header[1];
            this.RequestId = (header[2] << 8) + header[3];
            this.ContentLength = (header[4] << 8) + header[5];
            this.PaddingLength = header[6];
        }

        /// <summary>
        /// FastCGI protocol version
        /// </summary>
        public int Version;

        /// <summary>
        /// FastCGI record type
        /// </summary>
        public RecordType Type;

        /// <summary>
        /// Identifies the FastCGI request to which the record belongs
        /// </summary>
        public int RequestId;

        /// <summary>
        /// The number of bytes in the ContentData component of the record
        /// </summary>
        public int ContentLength;

        /// <summary>
        /// The number of bytes in the PaddingData component of the record
        /// </summary>
        public int PaddingLength;

        /// <summary>
        /// Record content data
        /// </summary>
        public byte[] ContentData;

        /// <summary>
        /// Record padding data
        /// </summary>
        public byte[] PaddingData;

        /// <summary>
        /// Returns the header for the record.
        /// </summary>
        /// <returns>Record header as a byte array.</returns>
        public byte[] GetHeader()
        {
            byte[] header = new byte[8];

            header[0] = (byte)this.Version;
            header[1] = (byte)this.Type;
            header[2] = (byte)(this.RequestId >> 8);
            header[3] = (byte)this.RequestId;
            header[4] = (byte)(this.ContentLength >> 8);
            header[5] = (byte)this.ContentLength;
            header[6] = (byte)this.PaddingLength;

            return header;
        }
    }
}
