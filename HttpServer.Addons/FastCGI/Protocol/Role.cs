
namespace HttpServer.Addons.FastCgi.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// FastCGI application roles.
    /// </summary>
    /// <remarks>
    /// <see cref="http://www.fastcgi.com/drupal/node/6?q=node/22#S6"/>
    /// </remarks>
    internal enum Role : byte
    {
        Responder = 1,
        Authorizer = 2,
        Filter = 3
    }
}
