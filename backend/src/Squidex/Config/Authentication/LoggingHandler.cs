// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Config.Authentication
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
               : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string logPath = "C:\\work\\Projects\\sis-com\\squidex\\backend\\src\\Squidex\\http.log";
            if (!File.Exists(logPath))
            {
                File.Create(logPath);
            }

            StreamWriter file = File.AppendText(logPath);
            file.WriteLine("Request:");
            file.WriteLine(request.ToString());
            if (request.Content != null)
            {
                file.WriteLine(await request.Content.ReadAsStringAsync());
            }

            file.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            file.WriteLine("Response:");
            file.WriteLine(response.ToString());
            if (response.Content != null)
            {
                file.WriteLine(await response.Content.ReadAsStringAsync());
            }

            file.WriteLine();
            file.Flush();
            file.Close();
            return response;
        }
    }
}
