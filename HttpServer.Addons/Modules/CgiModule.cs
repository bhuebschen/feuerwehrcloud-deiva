using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HttpServer.Addons.CGI;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Modules;

namespace HttpServer.Addons.Modules
{
    /// <summary>
    /// Executes CGI applications and serves the output to the client.
    /// </summary>
    public class CgiModule : IModule
    {
        private readonly string _homeDirectory;
        private readonly ICgiService _cgiService;

        private readonly object _readLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CgiModule"/> class.
        /// </summary>
        /// <param name="homeDirectory">The directory containing the CGI scripts.</param>
        /// <param name="cgiService">The CGI service to handle the scripts.</param>
        /// <exception cref="ArgumentNullException"><c>baseUri</c> or <c>basePath</c> is <c>null</c>.</exception>
        public CgiModule(string homeDirectory, ICgiService cgiService)
        {
            if (cgiService == null) throw new ArgumentNullException("cgiService");
            _cgiService = cgiService;

            if (!Directory.Exists(homeDirectory))
                throw new ArgumentException("Directory does not exist: " + homeDirectory, "homeDirectory");

            _homeDirectory = homeDirectory;
        }

        #region IModule Members

        /// <summary>
        /// Process a request.
        /// </summary>
        /// <param name="context">Request information</param>
        /// <returns>What to do next.</returns>
        /// <exception cref="InternalServerException">Failed to find file extension</exception>
        /// <exception cref="ForbiddenException">Forbidden file type.</exception>
        public ProcessingResult Process(RequestContext context)
        {
            var scriptName = CgiHelper.GetScriptName(context.Request.Uri, _cgiService.Extension);
            var filePath = scriptName.Replace("/", @"\").TrimStart('\\');
            var path = Path.Combine(_homeDirectory, filePath);

            if (String.IsNullOrEmpty(path))
                return ProcessingResult.Continue;

            IRequest request = context.Request;
            IResponse response = context.Response;

            try
            {
                string ext = Path.GetExtension(scriptName).TrimStart('.');

                if (!ext.Equals(_cgiService.Extension, StringComparison.OrdinalIgnoreCase))
                    return ProcessingResult.Continue;

                // TODO: is this a good place to lock?
                lock (_readLock)
                {
                    int code = _cgiService.Execute(context.HttpContext, path, scriptName);

                    string output = String.Empty;

                    // Check StandardError stream
                    if (_cgiService.HasError)
                    {
                        output = _cgiService.StandardError;
                    }
                    else
                    {
						output = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(_cgiService.StandardOutput));
						//System.Text.Encoding.de
                    }

                    // Set default content type
                    response.ContentType = new ContentTypeHeader("text/html");

                    ProcessHeaders(response, CgiHelper.ParseCgiHeaders(ref output));

                    response.ContentLength.Value = output.Length;

                    ResponseWriter generator = new ResponseWriter();
                    generator.SendHeaders(context.HttpContext, response);
                    generator.Send(context.HttpContext, output, CgiHelper.Encoding);
                }

                return ProcessingResult.Abort;
            }
            catch (FileNotFoundException err)
            {
                throw new InternalServerException("Failed to process file '" + request.Uri + "'.", err);
            }
            catch (Exception err)
            {
                throw new InternalServerException("Failed to process file '" + request.Uri + "'.", err);
            }
        }

        #endregion

        private void ProcessHeaders(IResponse response, Dictionary<string, string> headers)
        {
            foreach (var item in headers)
            {
                if (item.Key.Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    SetStatusCode(response, item.Key, item.Value);
                }
                else if (item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                {
                    response.ContentType = new ContentTypeHeader(item.Value);
                }
                else
                {
                    response.Add(new StringHeader(item.Key, item.Value));
                }
            }
        }

        private void SetStatusCode(IResponse response, string key, string value)
        {
            if (value.StartsWith("404"))
            {
                response.Status = HttpStatusCode.NotFound;
            }
            else if (value.StartsWith("401"))
            {
                response.Status = HttpStatusCode.Unauthorized;
            }
            else if (value.StartsWith("302"))
            {
                response.Status = HttpStatusCode.Found;
            }
        }
    }
}
