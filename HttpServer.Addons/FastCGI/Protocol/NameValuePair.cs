
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Describes a single FastCGI name-value pair.
    /// </summary>
    internal struct NameValuePair
    {
        /// <summary>
        /// Name of the name-value pair.
        /// </summary>
        public string Name;

        /// <summary>
        /// Value of the name-value pair.
        /// </summary>
        public string Value;

        /// <summary>
        /// Returns the header data for the name-value pair.
        /// </summary>
        /// <returns>Name value pair header as a byte array.</returns>
        public byte[] GetHeader()
        {
            // TODO: Encoding lengths in less than 4 bytes
            // TODO: Validation of string length
            int nameLength = this.Name.Length;
            int valueLength = this.Value == null ? 0 : this.Value.Length;
            byte[] returnValue = new byte[8];

            returnValue[0] = (byte)((0x80 | nameLength << 24));
            returnValue[1] = (byte)(nameLength << 16);
            returnValue[2] = (byte)(nameLength << 8);
            returnValue[3] = (byte)nameLength;

            returnValue[4] = (byte)((0x80 | valueLength << 24));
            returnValue[5] = (byte)(valueLength << 16);
            returnValue[6] = (byte)(valueLength << 8);
            returnValue[7] = (byte)valueLength;

            return returnValue;
        }
    }
}
