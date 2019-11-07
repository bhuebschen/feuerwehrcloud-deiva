using System;
using System.Text;
using HttpServer.Addons.Files;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Modules;

namespace HttpServer.Addons.Modules
{
    /// <summary>
    /// Serves directory listings.
    /// </summary>
    /// <remarks>
    /// TODO: Handle virtual directories.
    /// </remarks>
    public class DirectoryModule : IModule   {
        const string ERROR_MESSAGE = "Failed to build directory info";

        private readonly IFileService _fileService;

        private IDirectoryListing _listing;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryModule" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public DirectoryModule(IFileService fileService)
            : this(fileService, new DirectoryListing())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryModule" /> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public DirectoryModule(IFileService fileService, IDirectoryListing listing)
        {
            if (fileService == null) throw new ArgumentNullException("fileService");
            _fileService = fileService;

            _listing = listing;
        }

        public ProcessingResult Process(RequestContext context)
        {
            IRequest request = context.Request;
            IResponse response = context.Response;

            // only handle GET
            if (!context.Request.Method.StartsWith("GET", StringComparison.OrdinalIgnoreCase))
                return ProcessingResult.Continue;

            string output;

            // serve a directory
            if (TryGenerateDirectoryPage(context, out output))
            {
                response.ContentType = new ContentTypeHeader("text/html");
                response.ContentLength.Value = output.Length;

                ResponseWriter generator = new ResponseWriter();
                generator.SendHeaders(context.HttpContext, response);
                generator.Send(context.HttpContext, output, Encoding.UTF8);

                return ProcessingResult.Abort;
            }

            return ProcessingResult.Continue;
        }

        private bool TryGenerateDirectoryPage(RequestContext context, out string output)
        {
            output = ERROR_MESSAGE;

            var uri = context.Request.Uri;

            if (!_fileService.IsDirectory(uri))
            {
                return false;
            }

            try
            {
                output = _listing.GetListing(uri, _fileService.GetFiles(uri),
                    _fileService.GetDirectories(uri));

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
