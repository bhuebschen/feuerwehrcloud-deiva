// -----------------------------------------------------------------------
// <copyright file="ICgiService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons.CGI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ICgiService
    {
        string Application { get; }
        string Extension { get; }

        string StandardOutput { get; }
        string StandardError { get; }

        bool HasError { get; }

        bool HasTimedOut { get; }
        int TimeOut { get; set; }

        int Execute(IHttpContext context, string path, string scriptName);
        void Stop();
    }
}
