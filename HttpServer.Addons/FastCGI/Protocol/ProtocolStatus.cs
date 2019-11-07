
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Enumeration of FastCGI protocol status values.
    /// </summary>
    public enum ProtocolStatus
    {
        /// <summary>
        /// Indicates a normal end of request.
        /// </summary>
        RequestComplete = 0,

        /// <summary>
        /// Indicates that the FastCGI application is rejecting a new request. This happens when a Web server sends concurrent 
        /// requests over one connection to an application that is designed to process one request at a time per connection.
        /// </summary>
        CantMultiplex = 1,

        /// <summary>
        /// Indicates that the FastCGI application is rejecting a new request. This happens when the application runs out of 
        /// some resource, e.g. database connections.
        /// </summary>
        Overloaded = 2,

        /// <summary>
        /// Indicates that the FastCGI application is rejecting a new request. This happens when the Web server has specified 
        /// a role that is unknown to the application.
        /// </summary>
        UnknownRole = 3
    }
}
