// -----------------------------------------------------------------------
// <copyright file="CgiHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons.CGI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using HttpServer;
    using HttpServer.Headers;
    using HttpServer.Messages;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal class CgiHelper
    {
        internal static readonly Encoding Encoding = Encoding.UTF8; //GetEncoding(1252);

        private const string ServerSoftware = "DEIVA/1.0";
        private const string GatewayInterface = "CGI/1.1";
        private const string ServerProtocol = "HTTP/1.1";

        /// <summary>
        /// Gets the environment variables used by CGI.
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="physicalPath">Physical path (working directory)</param>
        /// <param name="scriptName">The script name</param>
        /// <returns></returns>
        internal static Dictionary<string, string> GetEnvironmentVariables(IHttpContext context,
            string physicalPath, string scriptName)
        {
            IRequest request = context.Request;

            var variables = new Dictionary<string, string>();

            string method = request.Method.ToString().ToUpperInvariant();
            
            // Not request-specific

            // The name and version of the information server software answering the request 
            // (and running the gateway). Format: name/version 
            variables.Add("SERVER_SOFTWARE", ServerSoftware);

            // The server's hostname, DNS alias, or IP address as it would appear in self-referencing URLs.
            variables.Add("SERVER_NAME", request.Uri.Host);

            // The revision of the CGI specification to which this server complies. Format: CGI/revision
            variables.Add("GATEWAY_INTERFACE", GatewayInterface);

            // Set to any value will prevent cgi (i.e. php) to show force-cgi-redirect security alert and quit.
            variables.Add("REDIRECT_STATUS", "1");

            // Variables specific to the request

            // The name and revision of the information protcol this request came in with. 
            // Format: protocol/revision
            variables.Add("SERVER_PROTOCOL", ServerProtocol);

            // The port number to which the request was sent.
            variables.Add("SERVER_PORT", request.Uri.Port.ToString());

            // The method with which the request was made. For HTTP, this is "GET", "HEAD", "POST", etc.
            variables.Add("REQUEST_METHOD", method);

            variables.Add("REQUEST_URI", request.Uri.PathAndQuery);

            string path = request.Uri.LocalPath.Substring(scriptName.Length);

            // The extra path information, as given by the client. In other words, scripts can be accessed 
            // by their virtual pathname, followed by extra information at the end of this path. The extra 
            // information is sent as PATH_INFO. This information should be decoded by the server if it comes 
            // from a URL before it is passed to the CGI script.
            variables.Add("PATH_INFO", path);

            // The directory from which web documents are served.
            //variables.Add("DOCUMENT_ROOT", "");

            // The server provides a translated version of PATH_INFO, which takes the path and does any 
            // virtual-to-physical mapping to it.
            if (!String.IsNullOrEmpty(path))
            {
                string translated = physicalPath.Substring(0, physicalPath.Length - scriptName.Length);
                translated = Path.Combine(translated, path.TrimStart('/').Replace('/', '\\'));
                variables.Add("PATH_TRANSLATED", translated);
            }

            // A virtual path to the script being executed, used for self-referencing URLs.
            variables.Add("SCRIPT_NAME", scriptName);

            // A virtual path to the script being executed, used for self-referencing URLs.
            variables.Add("SCRIPT_FILENAME", physicalPath);

            // The information which follows the ? in the URL which referenced this script. This is the 
            // query information. It should not be decoded in any fashion. This variable should always be 
            // set when there is query information, regardless of command line decoding.
            variables.Add("QUERY_STRING", request.Uri.Query.TrimStart('?'));

            if (context.RemoteEndPoint != null) // TODO: null
            {
                // The IP address of the remote host making the request.
                variables.Add("REMOTE_ADDR", context.RemoteEndPoint.Address.ToString());

                // Remote port.
                variables.Add("REMOTE_PORT", context.RemoteEndPoint.Port.ToString());
            }

            // If the server supports user authentication, and the script is protected, this is the username 
            // they have authenticated as.
            //variables.Add("REMOTE_USER", "");

            // The hostname making the request. If the server does not have this information, it should set 
            // REMOTE_ADDR and leave this unset.
            //variables.Add("REMOTE_HOST",  "");

            // If the HTTP server supports RFC 931 identification, then this variable will be set to the remote 
            // user name retrieved from the server. Usage of this variable should be limited to logging only.
            //variables.Add("REMOTE_IDENT", "");

            // For queries which have attached information, such as HTTP POST and PUT, this is the content 
            // type of the data.
            variables.Add("CONTENT_TYPE", GetHeaderValue(request.Headers, "Content-Type"));

            // The length of the said content as given by the client.
            if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                variables.Add("CONTENT_LENGTH", request.Body.Length.ToString());
            }
            else
            {
                variables.Add("CONTENT_LENGTH", "0");
            }

            // Header lines received from the client

            if (request.Cookies.Count > 0)
            {
                variables.Add("HTTP_COOKIE", PrepareCookieData(request.Cookies));
            }

            variables.Add("HTTP_USER_AGENT", GetHeaderValue(request.Headers, "User-Agent"));
            variables.Add("HTTP_REFERER", GetHeaderValue(request.Headers, "Referer"));
            variables.Add("HTTP_ACCEPT", GetHeaderValue(request.Headers, "Accept"));

            // Authorization

            // If the server supports user authentication, and the script is protected, this is the 
            // protocol-specific authentication method used to validate the user.
            //variables.Add("AUTH_TYPE", "");

            return variables;
        }

        internal static string GetHeaderValue(IHeaderCollection headers, string name)
        {
            if (headers[name] != null)
            {
                return headers[name].HeaderValue;
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets the actual script name from a given uri.
        /// </summary>
        /// <param name="uri">Requested uri.</param>
        /// <param name="extensions">File extension.</param>
        /// <returns></returns>
        /// <example>
        /// A valid CGI request could be http:/127.0.0.1/cgi-bin/script.php/foo/bar?q=value.
        /// In that case the script name should be /cgi-bin/script.php.
        /// </example>
        internal static string GetScriptName(Uri uri, string extension)
        {
            string localPath = uri.LocalPath;
            string ext = "." + extension.TrimStart('.');

            int len = ext.Length;
            int first = localPath.IndexOf(ext);
            int last = localPath.LastIndexOf(ext);

            if (first < 0)
            {
                return localPath;
            }

            if (first == last)
            {
                return localPath.Substring(0, first + len);
            }

            // Now if local path is something like /foo1.php.bar1.php/foo2/bar2
            // between is .bar1.php/foo2/bar2
            string between = localPath.Substring(0, last);
            between = between.Substring(first + len);

            if (!between.Contains("/"))
            {
                return localPath.Substring(0, last + len);
            }

            return localPath;
        }

        /// <summary>
        /// Prepares a string with form data for cgi input.
        /// </summary>
        /// <param name="cookies">Parameter collection</param>
        /// <returns></returns>
        internal static string PrepareFormData(IParameterCollection form)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in form)
            {
                sb.AppendFormat("{0}={1}&", item.Name, item.Value);
            }

            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        /// Prepares a string with cookie data for HTTP_COOKIE cgi variable.
        /// </summary>
        /// <param name="cookies">Cookie collection</param>
        /// <returns></returns>
        internal static string PrepareCookieData(RequestCookieCollection cookies)
        {
            StringBuilder sb = new StringBuilder();

            foreach (RequestCookie item in cookies)
            {
                sb.AppendFormat("{0}={1};", item.Name, item.Value);
            }

            return sb.ToString().TrimEnd(';');
        }

        /// <summary>
        /// Parse the headers sent back by CGI.
        /// </summary>
        /// <param name="output">The CGI output.</param>
        /// <returns></returns>
        internal static Dictionary<string, string> ParseCgiHeaders(ref string output)
        {
            var headers = new Dictionary<string, string>();

            // TODO: Make this more robust (are we really stripping headers???)
            int index = output.IndexOf("\r\n\r\n");

            if (index != -1)
            {
                string header = output.Substring(0, index + 2);
                output = output.Substring(index + 2);

                int end = header.IndexOf("\r\n");

                while (end != -1)
                {
                    string line = header.Substring(0, end);
                    header = header.Substring(end + 2);

                    int colonIndex = line.IndexOf(":");
                    if (colonIndex <= 1)
                        break;

                    string name = line.Substring(0, colonIndex).Trim();
                    string val = line.Substring(colonIndex + 1).Trim();

                    headers.Add(name, val);

                    end = header.IndexOf("\r\n");
                }
            }

            output = output.Trim();

            return headers;
        }
    }
}
