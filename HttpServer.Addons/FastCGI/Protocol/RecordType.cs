
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// FastCGI record types.
    /// </summary>
    /// <remarks>
    /// <see cref="http://www.fastcgi.com/drupal/node/6?q=node/22#S5"/>
    /// </remarks>
    internal enum RecordType : byte
    {
        None = 0,
        BeginRequest = 1,
        AbortRequest = 2,
        EndRequest = 3,
        Params = 4,
        StandardInput = 5,
        StandardOutput = 6,
        StandardError = 7,
        Data = 8,
        GetValues = 9,
        GetValuesResult = 10,
        UnknownType = 11
    }
}
