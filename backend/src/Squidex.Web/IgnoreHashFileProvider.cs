// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Squidex.Web;

public sealed partial class IgnoreHashFileProvider : IFileProvider
{
    private readonly char[] pathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '\\'];
    private readonly Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly IFileProvider inner;

    public IgnoreHashFileProvider(IFileProvider inner)
    {
        this.inner = inner;

        var regex = BuildFileWithHashRegex();

        void MapDirectory(string path)
        {
            foreach (var file in inner.GetDirectoryContents(path))
            {
                if (file.IsDirectory)
                {
                    MapDirectory(Combine(path, file.Name));
                    continue;
                }

                var match = regex.Match(file.Name);

                if (match.Success)
                {
                    var nameWithouthHash = $"{match.Groups["Name"].Value}.{match.Groups["Extension"].Value}";

                    var pathHashed = Combine(path, file.Name);
                    var pathNormal = Combine(path, nameWithouthHash);

                    map[pathNormal] = pathHashed;
                }
            }
        }

        MapDirectory(string.Empty);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var file = inner.GetFileInfo(subpath);

        if (!file.Exists)
        {
            subpath = subpath.TrimStart(pathSeparators).Replace('\\', '/');

            if (map.TryGetValue(subpath, out var withHash))
            {
                file = inner.GetFileInfo(withHash);
            }
        }

        return file;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return inner.GetDirectoryContents(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        return inner.Watch(filter);
    }

    private static string Combine(string path1, string path2)
    {
        if (string.IsNullOrWhiteSpace(path1))
        {
            return path2;
        }

        if (string.IsNullOrWhiteSpace(path2))
        {
            return path1;
        }

        return $"{path1}/{path2}";
    }

    [GeneratedRegex("^(?<Name>[^.]+)(\\.|-)[0-9A-Za-z]{4,}\\.(?<Extension>.+)$")]
    private static partial Regex BuildFileWithHashRegex();
}
