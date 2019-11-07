
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Describes the body of a FastCGI begin request record.
    /// </summary>
    internal struct BeginRequestBody
    {
        private const int FCGI_KEEP_CONN = 1;

        /// <summary>
        /// The role the Web server expects the application to play.
        /// </summary>
        public Role Role;

        /// <summary>If false, the FastCGI application closes the connection after responding 
        /// to this request.  If true, the child FastCGI application does not close the connection after responding 
        /// to this request; the Web server retains responsibility for the connection.</summary>
        public bool KeepConnection;

        /// <summary>
        /// Returns the record body.
        /// </summary>
        /// <returns>Record body as a byte array.</returns>
        public byte[] ToByteArray()
        {
            byte[] returnValue = new byte[8];

            returnValue[1] = (byte)this.Role;
            byte flags = 0;
            if (this.KeepConnection)
            {
                flags |= FCGI_KEEP_CONN;
            }
            returnValue[2] = flags;

            return returnValue;
        }
    }
}
