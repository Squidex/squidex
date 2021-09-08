// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Scripting
{
    public sealed class ScriptingCompletion
    {
        private readonly Stack<string> prefixes = new Stack<string>();
        private readonly HashSet<ScriptingValue> result = new HashSet<ScriptingValue>();

        public IReadOnlyList<ScriptingValue> Content(Schema schema, PartitionResolver partitionResolver)
        {
            AddObject("ctx", FieldDescriptions.Context, () =>
            {
                AddFunction("replace()",
                    "Tell Squidex that you have modified the data and that the change should be applied.");

                AddFunction("getReferences(ids, callback)",
                    "Queries the content items with the specified IDs and invokes the callback with an array of contents.");

                AddFunction("getReference(ids, callback)",
                    "Queries the content item with the specified ID and invokes the callback with an array of contents.");

                AddFunction("getAssets(ids, callback)",
                    "Queries the assets with the specified IDs and invokes the callback with an array of assets.");

                AddFunction("getAsset(ids, callback)",
                    "Queries the asset with the specified ID and invokes the callback with an array of assets.");

                AddShared();

                AddString("contentId",
                    FieldDescriptions.EntityId);

                AddString("status",
                    FieldDescriptions.ContentStatus);

                AddString("statusOld",
                    FieldDescriptions.ContentStatusOld);

                AddObject("data", FieldDescriptions.ContentData, () =>
                {
                    AddData(schema, partitionResolver);
                });

                AddObject("dataOld", FieldDescriptions.ContentDataOld, () =>
                {
                    AddData(schema, partitionResolver);
                });
            });

            return result.OrderBy(x => x.Path).ToList();
        }

        public IReadOnlyList<ScriptingValue> Asset()
        {
            AddObject("ctx", FieldDescriptions.Context, () =>
            {
                AddShared();

                AddString("assetId",
                    FieldDescriptions.EntityId);

                AddObject("asset",
                    FieldDescriptions.Asset, () =>
                {
                    AddSharedAsset();

                    AddNumber("fileVersion",
                        FieldDescriptions.AssetFileVersion);
                });

                AddObject("command",
                    FieldDescriptions.Command, () =>
                {
                    AddSharedAsset();

                    AddBoolean("permanent",
                        FieldDescriptions.EntityRequestDeletePermanent);
                });
            });

            return result.OrderBy(x => x.Path).ToList();
        }

        private void AddSharedAsset()
        {
            AddString("fileHash",
                FieldDescriptions.AssetFileHash);

            AddString("fileName",
                FieldDescriptions.AssetFileName);

            AddString("fileSize",
                FieldDescriptions.AssetFileSize);

            AddString("fileSlug",
                FieldDescriptions.AssetSlug);

            AddString("mimeType",
                FieldDescriptions.AssetMimeType);

            AddBoolean("isProtected",
                FieldDescriptions.AssetIsProtected);

            AddString("parentId",
                FieldDescriptions.AssetParentId);

            AddArray("parentPath",
                FieldDescriptions.AssetParentPath);

            AddArray("tags",
                FieldDescriptions.AssetTags);

            AddObject("metadata",
                FieldDescriptions.AssetMetadata, () =>
            {
                AddArray("name",
                    FieldDescriptions.AssetMetadataValue);
            });
        }

        private void AddShared()
        {
            AddFunction("disallow()",
                "Tell Squidex to not allow the current operation and to return a 403 (Forbidden).");

            AddFunction("reject('Reason')",
                "Tell Squidex to reject the current operation and to return a 403 (Forbidden).");

            AddFunction("html2Text(text)",
                "Converts a HTML string to plain text.");

            AddFunction("markdown2Text(text)",
                "Converts a markdown string to plain text.");

            AddFunction("formatDate(data, pattern)",
                "Formats a JavaScript date object using the specified pattern.");

            AddFunction("formatTime(text)",
                "Formats a JavaScript date object using the specified pattern.");

            AddFunction("wordCount(text)",
                "Counts the number of words in a text. Useful in combination with html2Text or markdown2Text.");

            AddFunction("characterCount(text)",
                "Counts the number of characters in a text. Useful in combination with html2Text or markdown2Text.");

            AddFunction("toCamelCase(text)",
                "Converts a text to camelCase.");

            AddFunction("toPascalCase(text)",
                "Calculate the SHA256 hash from a given string. Use this method for hashing passwords");

            AddFunction("sha256(text)",
                "Calculate the MD5 hash from a given string. Use this method for hashing passwords, when backwards compatibility is important.");

            AddFunction("slugify(text)",
                "Calculates the slug of a text by removing all special characters and whitespaces to create a friendly term that can be used for SEO-friendly URLs.");

            AddFunction("slugify(text)",
                "Calculates the slug of a text by removing all special characters and whitespaces to create a friendly term that can be used for SEO-friendly URLs.");

            AddFunction("slugify(text)",
                "Calculates the slug of a text by removing all special characters and whitespaces to create a friendly term that can be used for SEO-friendly URLs.");

            AddFunction("slugify(text)",
                "Calculates the slug of a text by removing all special characters and whitespaces to create a friendly term that can be used for SEO-friendly URLs.");

            AddFunction("getJSON(url, callback, ?headers)",
                "Makes a GET request to the defined URL and parses the result as JSON. Headers are optional.");

            AddFunction("postJSON(url, body, callback, ?headers)",
                "Makes a POST request to the defined URL and parses the result as JSON. Headers are optional.");

            AddFunction("putJSON(url, body, callback, ?headers)",
                "Makes a PUT request to the defined URL and parses the result as JSON. Headers are optional.");

            AddFunction("putJSON(url, body, callback, ?headers)",
                "Makes a PUT request to the defined URL and parses the result as JSON. Headers are optional.");

            AddFunction("patchJSON(url, body, callback, headers)",
                "Makes a PATCH request to the defined URL and parses the result as JSON. Headers are optional.");

            AddFunction("deleteJSON(url, body, callback, headers)",
                "Makes a DELETE request to the defined URL and parses the result as JSON. Headers are optional.");

            AddString("appId",
                FieldDescriptions.AppId);

            AddString("appName",
                FieldDescriptions.AppName);

            AddString("operation",
                FieldDescriptions.Operation);

            AddObject("user",
                FieldDescriptions.User, () =>
            {
                AddString("id",
                    FieldDescriptions.UserId);

                AddString("email",
                    FieldDescriptions.UserEmail);

                AddBoolean("isClient",
                    FieldDescriptions.UserIsClient);

                AddBoolean("isUser",
                    FieldDescriptions.UserIsUser);

                AddObject("claims",
                    FieldDescriptions.UserClaims, () =>
                {
                    AddArray("name",
                        FieldDescriptions.UsersClaimsValue);
                });
            });
        }

        private void AddData(Schema schema, PartitionResolver partitionResolver)
        {
            foreach (var field in schema.Fields.Where(x => x.IsForApi(true)))
            {
                var description = $"The values of the '{field.DisplayName()}' field.";

                AddObject(field.Name, $"The values of the '{field.DisplayName()}' field.", () =>
                {
                    foreach (var partition in partitionResolver(field.Partitioning).AllKeys)
                    {
                        var description = $"The '{partition}' value of the '{field.DisplayName()}' field.";

                        if (field is ArrayField arrayField)
                        {
                            AddObject(partition, description, () =>
                            {
                                foreach (var nestedField in arrayField.Fields.Where(x => x.IsForApi(true)))
                                {
                                    var description = $"The value of the '{nestedField.DisplayName()}' nested field.";

                                    AddAny(field.Name, description);
                                }
                            });
                        }
                        else
                        {
                            AddAny(partition, description);
                        }
                    }
                });
            }
        }

        private void AddAny(string name, string description)
        {
            Add(JsonType.Any, name, description);
        }

        private void AddArray(string name, string description)
        {
            Add(JsonType.Array, name, description);
        }

        private void AddBoolean(string name, string description)
        {
            Add(JsonType.Boolean, name, description);
        }

        private void AddFunction(string name, string description)
        {
            Add(JsonType.Function, name, description);
        }

        private void AddNumber(string name, string description)
        {
            Add(JsonType.Number, name, description);
        }

        private void AddString(string name, string description)
        {
            Add(JsonType.String, name, description);
        }

        private void Add(JsonType type, string name, string description)
        {
            var fullName = string.Join('.', prefixes.Reverse().Union(Enumerable.Repeat(name, 1)));

            result.Add(new ScriptingValue(fullName, type, description));
        }

        private void AddObject(string name, string description, Action inner)
        {
            Add(JsonType.Object, description, name);

            prefixes.Push(name);
            try
            {
                inner();
            }
            finally
            {
                prefixes.Pop();
            }
        }
    }
}
