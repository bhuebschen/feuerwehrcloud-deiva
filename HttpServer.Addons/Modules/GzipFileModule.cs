using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using HttpServer.Addons.Files;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Modules;

namespace HttpServer.Addons.Modules
{
    /// <summary>
    /// Serves files in the web server.
    /// </summary>
    /// <example>
    /// <code>
    /// var fileService = new DiskFileService("/", path);
    /// var fileModule = new GzipFileModule(fileService) { EnableGzip = true };
    /// </code>
    /// </example>
    public class GzipFileModule : IModule
    {
        private readonly IFileService _fileService;
        private bool _enableGzip;
        private List<string> _gzipCandidates;

        public bool EnableGzip
        {
            get { return _enableGzip; }
            set { _enableGzip = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GzipFileModule" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public GzipFileModule(IFileService fileService)
        {
            if (fileService == null) throw new ArgumentNullException("fileService");
            _fileService = fileService;

            _enableGzip = false;
            _gzipCandidates = new List<string>() { "html", "css", "js", "json", "xml", "txt", "htm" };
        }

        /// <summary>
        /// Will send a file to client.
        /// </summary>
        /// <param name="context">HTTP context containing outbound stream.</param>
        /// <param name="response">Response containing headers.</param>
        /// <param name="stream">File stream</param>
        private void SendFile(IHttpContext context, IResponse response, Stream stream)
        {
            response.ContentLength.Value = stream.Length;

            ResponseWriter generator = new ResponseWriter();
            generator.SendHeaders(context, response);
            generator.SendBody(context, stream);
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
            IRequest request = context.Request;
            IResponse response = context.Response;

            try
            {
                // only handle GET and HEAD
                if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                    && !request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                    return ProcessingResult.Continue;

                if (_fileService.IsDirectory(request.Uri))
                    return ProcessingResult.Continue;

                var header = request.Headers["If-Modified-Since"];
                var time = header != null
                               ? DateTime.ParseExact(header.HeaderValue, "R", CultureInfo.InvariantCulture)
                               : DateTime.MinValue;

                DateTime since = time.ToUniversalTime();
                var fileContext = new FileContext(context.Request, since);
                _fileService.GetFile(fileContext);
                if (!fileContext.IsFound)
                {
                    response.Status = HttpStatusCode.NotFound;
                    return ProcessingResult.SendResponse;
                }

                if (!fileContext.IsModified)
                {
                    response.Status = HttpStatusCode.NotModified;
                    return ProcessingResult.SendResponse;
                }

                var mimeType = MimeTypeProvider.Instance.Get(fileContext.Filename);
                if (mimeType == null)
                {
                    response.Status = HttpStatusCode.UnsupportedMediaType;
                    return ProcessingResult.SendResponse;
                }

                if (mimeType.Equals("application/x-httpd-cgi"))
                {
                    // CGI module handles the request
                    return ProcessingResult.Continue;
                }

                response.Status = HttpStatusCode.OK;
                response.ContentType = new ContentTypeHeader(mimeType);
                response.Add(new DateHeader("Last-Modified", fileContext.LastModifiedAtUtc));

                IHeader acceptEncodingHeader = request.Headers["Accept-Encoding"];

                if (_enableGzip
                    && fileContext.FileStream.Length > 5000
                    && acceptEncodingHeader != null
                    && acceptEncodingHeader.HeaderValue.Contains("gzip")
                    && IsGzipCandidate(fileContext.Filename))
                {
                    // Compress file (if larger than 5kb)
                    var stream = GetGzipStream(fileContext.FileStream);

                    response.Add(new StringHeader("Content-Encoding", "gzip"));

                    SendFile(context.HttpContext, response, stream);
                }
                else
                {
                    SendFile(context.HttpContext, response, fileContext.FileStream);
                }

                // Release file stream
                fileContext.FileStream.Close();

                return ProcessingResult.Abort;
            }
            catch (FileNotFoundException err)
            {
                throw new InternalServerException("Failed to process file '" + request.Uri + "'.", err);
            }
        }

        #endregion

        private Stream GetGzipStream(Stream fileStream)
        {
            // The stream to hold the gzipped bytes.
            MemoryStream outStream = new MemoryStream();

            // Create gzip stream (have to leave the output stream open)
            using (GZipStream gz = new GZipStream(outStream, CompressionMode.Compress, true))
            {
                fileStream.CopyTo(gz);

                // Have to call close (flush doesn't always work), so everything gets copied 
                // to the memory stream ...
                gz.Close();
            }

            // Reposition stream
            outStream.Position = 0;

            return outStream;
        }

        private bool IsGzipCandidate(string filename)
        {
            foreach (var item in _gzipCandidates)
            {
                if (filename.EndsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
