using System;
using System.Collections.Generic;
using System.IO;
using HttpServer.Routing;

namespace HttpServer.Addons.Routing
{
    public class DefaultIndexRouter : IRouter
    {
        private readonly string _homeDirectory;

        private readonly List<string> _defaultIndex;
        private bool _serveDefaultIndex;

        /// <summary>
        /// Initializes innstance of the <see cref="DefaultIndexRouter" /> class.
        /// </summary>
        public DefaultIndexRouter(string homeDirectory)
            : this(homeDirectory, "index.html")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIndexRouter" /> class.
        /// </summary>
        public DefaultIndexRouter(string homeDirectory, params string[] defaultIndex)
        {
            if (homeDirectory == null) throw new ArgumentNullException("homeDirectory");
            if (!Directory.Exists(homeDirectory))
                throw new ArgumentException("Directory does not exist: " + homeDirectory, "homeDirectory");

            _homeDirectory = homeDirectory;

            if (defaultIndex == null) throw new ArgumentNullException("defaultIndex");
            _defaultIndex = new List<string>(defaultIndex);

            _serveDefaultIndex = true;
        }

        public void AddIndexFile(string filename)
        {
            _defaultIndex.Add(filename);
        }

        public ProcessingResult Process(RequestContext context)
        {
            if (!_serveDefaultIndex)
            {
                return ProcessingResult.SendResponse;
            }

            IRequest request = context.Request;
            IResponse response = context.Response;

            var askedPath = request.Uri.AbsolutePath.Replace("/", @"\").TrimStart('\\');
            askedPath = Path.Combine(_homeDirectory, askedPath);

            if (!Directory.Exists(askedPath))
                return ProcessingResult.Continue;

            foreach (var _documentName in _defaultIndex)
            {
                var diskPath = Path.Combine(askedPath, _documentName);

                if (File.Exists(diskPath))
                {
                    var url = request.Uri.Scheme + "://" + request.Uri.Host;
                    if (request.Uri.Port != 80)
                    {
                        url += ":" + request.Uri.Port;
                    }

                    url += request.Uri.AbsolutePath.TrimEnd('/');
                    url += "/" + _documentName;

                    if (!string.IsNullOrEmpty(request.Uri.Query))
                    {
                        url += "?" + request.Uri.Query;
                    }

                    request.Uri = new Uri(url);
                    break;
                }
            }

            return ProcessingResult.Continue;
        }
    }
}
