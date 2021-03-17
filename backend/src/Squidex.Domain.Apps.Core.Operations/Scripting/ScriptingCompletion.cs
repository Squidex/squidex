// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptingCompletion
    {
        private readonly Stack<string> prefixes = new Stack<string>();
        private readonly HashSet<(string, string)> result = new HashSet<(string, string)>();

        public IReadOnlyList<(string Name, string Description)> GetCompletion(Schema schema, PartitionResolver partitionResolver)
        {
            Push("ctx", "The context object holding all values.");

            Add("appId", "The ID of the current app.");
            Add("appName", "The name of the current app.");
            Add("contentId", "The ID of the content item.");
            Add("operation", "The currnet query operation.");
            Add("status", "The status of the content item");
            Add("statusOld", "The old status of the content item.");

            Push("user", "Information about the current user.");
            Add("id", "The ID of the user.");
            Add("claims", "The additional properties of the user.");
            Add("claims.key", "The additional property of the user with name 'key'.");
            Add("claims['key']", "The additional property of the user with name 'key'.");
            Add("email", "The email address of the current user.");
            Add("isClient", "True when the current user is a client.");
            Pop();

            Push("data", "The data of the content item.");
            AddData(schema, partitionResolver);
            Pop();

            Push("oldData", "The old data of the content item.");
            AddData(schema, partitionResolver);
            Pop();

            Pop();

            Add("replace()",
                "Tell Squidex that you have modified the data and that the change should be applied.");

            Add("disallow()",
                "Tell Squidex to not allow the current operation and to return a 403 (Forbidden).");

            Add("reject('Reason')",
                "Tell Squidex to reject the current operation and to return a 403 (Forbidden).");

            return result.OrderBy(x => x.Item1).ToList();
        }

        private void AddData(Schema schema, PartitionResolver partitionResolver)
        {
            foreach (var field in schema.Fields.Where(x => x.IsForApi(true)))
            {
                Push(field.Name, $"The values of the '{field.DisplayName()}' field.");

                foreach (var partition in partitionResolver(field.Partitioning).AllKeys)
                {
                    Push(partition, $"The '{partition}' value of the '{field.DisplayName()}' field.");

                    if (field is ArrayField arrayField)
                    {
                        foreach (var nestedField in arrayField.Fields.Where(x => x.IsForApi(true)))
                        {
                            Push(field.Name, $"The value of the '{nestedField.DisplayName()}' nested field.");
                            Pop();
                        }
                    }

                    Pop();
                }

                Pop();
            }
        }

        private void Add(string name, string description)
        {
            result.Add((string.Join('.', prefixes.Reverse().Union(Enumerable.Repeat(name, 1))), description));
        }

        private void Push(string prefix, string description)
        {
            Add(prefix, description);

            prefixes.Push(prefix);
        }

        private void Pop()
        {
            prefixes.Pop();
        }
    }
}
