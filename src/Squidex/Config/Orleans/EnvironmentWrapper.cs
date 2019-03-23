// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Squidex.Config.Orleans
{
    public sealed class EnvironmentWrapper : IHostingEnvironment
    {
        private readonly IWebHostEnvironment nested;

        public string EnvironmentName
        {
            get => nested.EnvironmentName;
            set => nested.EnvironmentName = value;
        }

        public string ApplicationName
        {
            get => nested.ApplicationName;
            set => nested.ApplicationName = value;
        }

        public string ContentRootPath
        {
            get => nested.ContentRootPath;
            set => nested.ContentRootPath = value;
        }

        public IFileProvider ContentRootFileProvider
        {
            get => nested.ContentRootFileProvider;
            set => nested.ContentRootFileProvider = value;
        }

        public EnvironmentWrapper(IWebHostEnvironment nested)
        {
            this.nested = nested;
        }
    }
}
