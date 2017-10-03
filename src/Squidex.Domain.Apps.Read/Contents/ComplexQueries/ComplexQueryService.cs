// ==========================================================================
//  ComplexQueryService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Contents.ComplexQueries
{
    public sealed class ComplexQueryService
    {
        private readonly IContentQueryService contentQuery;
        private readonly IAssetRepository assetRepository;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(2000);

        public ComplexQueryService(IContentQueryService contentQuery, IAssetRepository assetRepository)
        {
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(assetRepository, nameof(assetRepository));

            this.contentQuery = contentQuery;

            this.assetRepository = assetRepository;
        }

        public Task<JToken> QueryAsync(IAppEntity appEntity, ClaimsPrincipal user, string scripts, string function)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(appEntity, nameof(appEntity));
            Guard.NotNullOrEmpty(scripts, nameof(scripts));
            Guard.NotNullOrEmpty(function, nameof(function));

            var queryContent = new JintQueryContext(appEntity, assetRepository, contentQuery, user);

            var engine = new Engine(options => options.TimeoutInterval(Timeout).Strict());

            var cts = new TaskCompletionSource<JToken>();

            EnableDisallow(engine);
            EnableReject(engine);

            engine.SetValue("cb", new Action<JsValue>(result =>
            {
                cts.SetResult(JsonMapper.Map(result));
            }));

            engine.Execute(scripts + Environment.NewLine + Environment.NewLine + function);

            try
            {
            }
            catch (Exception ex)
            {
                cts.SetException(ex);
            }

            return cts.Task;
        }

        private static void EnableDisallow(Engine engine)
        {
            engine.SetValue("disallow", new Action<string>(message =>
            {
                var exMessage = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

                throw new DomainForbiddenException(exMessage);
            }));
        }

        private static void EnableReject(Engine engine)
        {
            engine.SetValue("reject", new Action<string>(message =>
            {
                var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

                throw new ValidationException($"Script rejected the operation.", errors);
            }));
        }
    }
}
