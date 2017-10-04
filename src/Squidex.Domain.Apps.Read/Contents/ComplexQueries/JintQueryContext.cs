// ==========================================================================
//  JintQueryContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;

namespace Squidex.Domain.Apps.Read.Contents.ComplexQueries
{
    public sealed class JintQueryContext : QueryContext
    {
        public JintQueryContext(IAppEntity app, IAssetRepository assetRepository, IContentQueryService contentQuery, ClaimsPrincipal user)
            : base(app, assetRepository, contentQuery, user)
        {
        }

        public void Setup(Engine engine)
        {
            AddFindContent(engine);
            AddQueryContents(engine);
            AddQueryContentsByIds(engine);
            AddContext(engine);
        }

        private void AddFindContent(Engine engine)
        {
            var findContent = new Action<string, string, Action<JsValue>>((schemaIdOrName, id, callback) =>
            {
                if (!TryParseGuids(new[] { id }, out var guids))
                {
                    callback(null);
                }

                FindContentAsync(schemaIdOrName, guids[0]).ContinueWith(task =>
                {
                    callback(task.Status == TaskStatus.RanToCompletion
                        ? new JintContent(engine, task.Result)
                        : null);
                });
            });

            engine.SetValue("findContent", findContent);
        }

        private void AddQueryContentsByIds(Engine engine)
        {
            var findContent = new Action<string, string[], Action<JsValue>>((schemaIdOrName, ids, callback) =>
            {
                if (!TryParseGuids(ids, out var guids))
                {
                    callback(null);
                }

                QueryContentsAsync(schemaIdOrName, guids).ContinueWith(task =>
                {
                    callback(task.Status == TaskStatus.RanToCompletion
                        ? engine.Array.Construct(task.Result.Select(t => new JsValue(new JintContent(engine, t))).ToArray())
                        : null);
                });
            });

            engine.SetValue("queryContentByIds", findContent);
        }

        private void AddQueryContents(Engine engine)
        {
            var findContent = new Action<string, string, Action<JsValue>>((schemaIdOrName, query, callback) =>
            {
                QueryContentsAsync(schemaIdOrName, query).ContinueWith(task =>
                {
                    callback(task.Status == TaskStatus.RanToCompletion
                        ? engine.Array.Construct(task.Result.Select(t => new JsValue(new JintContent(engine, t))).ToArray())
                        : null);
                });
            });

            engine.SetValue("queryContentByIds", findContent);
        }

        private static bool TryParseGuids(string[] ids, out List<Guid> result)
        {
            result = new List<Guid>();

            foreach (var id in ids)
            {
                if (!Guid.TryParse(id, out var guid))
                {
                    result = null;

                    return false;
                }

                result.Add(guid);
            }

            return true;
        }

        private void AddContext(Engine engine)
        {
            var context = new ObjectInstance(engine);

            if (User != null)
            {
                context.FastAddProperty("user", new JintUser(engine, User), false, false, false);
            }

            engine.SetValue("ctx", context);
        }
    }
}
