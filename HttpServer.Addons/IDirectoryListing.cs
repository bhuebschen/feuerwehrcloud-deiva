// -----------------------------------------------------------------------
// <copyright file="IDirectoryListing.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons
{
    using System;
    using System.Collections.Generic;
    using HttpServer.Addons.Files;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDirectoryListing
    {
        string GetListing(Uri uri, IEnumerable<FileInformation> files, IEnumerable<string> directories);
    }
}
