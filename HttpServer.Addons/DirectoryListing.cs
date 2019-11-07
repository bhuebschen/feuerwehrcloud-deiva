// -----------------------------------------------------------------------
// <copyright file="DirectoryListing.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using HttpServer.Addons.Files;

    /// <summary>
    /// A simple directory listing template.
    /// </summary>
    public class DirectoryListing : IDirectoryListing
    {
        private static NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;

        string _listing;
        string _files;

        Stopwatch _stopwatch;

        public DirectoryListing()
            : this(DefaultListing, DefaultFiles)
        {
        }

        public DirectoryListing(string listing, string files)
        {
            this._listing = listing;
            this._files = files;

            _stopwatch = new Stopwatch();
        }

        public string GetListing(Uri uri, IEnumerable<FileInformation> files,
            IEnumerable<string> directories)
        {
            _stopwatch.Start();

            string template = _listing.Replace("{{Uri}}", uri.LocalPath);
            template = template.Replace("{{ServerName}}", "HTTPServer/2.0");
            template = template.Replace("{{ServerTime}}", DateTime.Now.ToString());

            StringBuilder sb = new StringBuilder();

            string path = uri.LocalPath.TrimEnd('/') + "/";
            string clean, up = "..";

            foreach (var dir in directories)
            {
                if (dir == up)
                {
                    continue;
                }

                // Directories start with "\", so clean up:
                clean = dir.TrimStart('\\');

                sb.Append(GetFile(path + clean + "/", clean, "", "<span class=\"d\">Folder<span>"));
            }

            double size = 0.0;

            foreach (var file in files)
            {
                size = file.Size / 1024.0;

                sb.Append(GetFile(path + file.Name, file.Name,
                    size.ToString("0.00", nfi) + " KB", file.LastModifiedAtUtc.ToString()));
            }

            template = template.Replace("{{Files}}", sb.ToString());

            _stopwatch.Stop();

            return template.Replace("{{LoadTime}}", _stopwatch.ElapsedMilliseconds + "ms");
        }

        private string GetFile(string path, string name, string size, string date)
        {
            string template = _files.Replace("{{Path}}", path);
            template = template.Replace("{{Name}}", name);
            template = template.Replace("{{Size}}", size);
            return template.Replace("{{Date}}", date);
        }

        private string GetParentPath(Uri uri)
        {
            string uriPathParent = uri.LocalPath.TrimEnd('/');

            int pos = uriPathParent.LastIndexOf('/') + 1;

            if (pos > 0)
            {
                uriPathParent = uriPathParent.Substring(0, pos);
            }

            return uriPathParent;
        }

        #region Default template

        private static string DefaultListing = @"<!doctype html>
<html lang=""en"">
<head>
    <title>Index of {{Uri}}</title>
    <meta charset=""utf-8"">
    <style>
    body { background: white; font: normal 10pt 'Segoe UI',sans-serif; }
    h1 { margin-bottom: 10px; font-size: 1.5em; }
    a { text-decoration: none; color: #026; }
    a:visited { color: #68a; }
    a:hover, a:focus { color: #820; }
    table {margin-left: 10px; font-size: 1em; }
    th, td { text-align: left; margin: 0; padding: 0; padding-right: 20px; }
    th { font-weight: bold; padding-bottom: 5px; }
    .c { min-width: 200px; }
    .r { text-align: right; }
    .d { color: #787878; }
    .foot { color: #787878; padding-top: 7px; }
    </style>
</head>
<body>
    <h1>Index of {{Uri}}</h1>
    <table>
        <thead>
            <tr>
                <th class=""c"">Name</th>
                <th class=""r"">Size</th>
                <th>Last modified</th>
            </tr>
        </thead>
        <tbody>{{Files}}
        </tbody>
    </table>
    <p class=""foot"">Generated in {{LoadTime}}, {{ServerName}}, {{ServerTime}}</p>
</body>
</html>";

        private static string DefaultFiles = @"
            <tr>
                <td><a href=""{{Path}}"">{{Name}}</a></td>
                <td class=""r"">{{Size}}</td>
                <td>{{Date}}</td>
            </tr>";

        #endregion
    }
}
