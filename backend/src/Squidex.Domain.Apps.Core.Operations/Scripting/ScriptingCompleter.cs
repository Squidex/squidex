// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptingCompleter
    {
        private readonly IEnumerable<IScriptDescriptor> descriptors;

        public ScriptingCompleter(IEnumerable<IScriptDescriptor> descriptors)
        {
            this.descriptors = descriptors;
        }

        public IReadOnlyList<ScriptingValue> ContentScript(FilterSchema dataSchema)
        {
            Guard.NotNull(dataSchema, nameof(dataSchema));

            return new Process(descriptors).Content(dataSchema, ScriptScope.ContentScript | ScriptScope.Transform);
        }

        public IReadOnlyList<ScriptingValue> ContentTrigger(FilterSchema dataSchema)
        {
            Guard.NotNull(dataSchema, nameof(dataSchema));

            return new Process(descriptors).Content(dataSchema, ScriptScope.ContentTrigger);
        }

        public IReadOnlyList<ScriptingValue> AssetScript()
        {
            return new Process(descriptors).Asset(ScriptScope.AssetScript);
        }

        public IReadOnlyList<ScriptingValue> AssetTrigger()
        {
            return new Process(descriptors).Asset(ScriptScope.AssetTrigger);
        }

        private sealed class Process
        {
            private readonly Stack<string> prefixes = new Stack<string>();
            private readonly HashSet<ScriptingValue> result = new HashSet<ScriptingValue>();
            private readonly IEnumerable<IScriptDescriptor> descriptors;

            public Process(IEnumerable<IScriptDescriptor> descriptors)
            {
                this.descriptors = descriptors;
            }

            public IReadOnlyList<ScriptingValue> Content(FilterSchema dataSchema, ScriptScope scope)
            {
                AddShared(scope);

                AddObject("ctx", FieldDescriptions.Context, () =>
                {
                    AddString("contentId",
                        FieldDescriptions.EntityId);

                    AddString("status",
                        FieldDescriptions.ContentStatus);

                    AddString("statusOld",
                        FieldDescriptions.ContentStatusOld);

                    AddObject("data", FieldDescriptions.ContentData, () =>
                    {
                        AddData(dataSchema);
                    });

                    AddObject("dataOld", FieldDescriptions.ContentDataOld, () =>
                    {
                        AddData(dataSchema);
                    });
                });

                return result.OrderBy(x => x.Path).ToList();
            }

            public IReadOnlyList<ScriptingValue> Asset(ScriptScope scope)
            {
                AddShared(scope);

                AddObject("ctx", FieldDescriptions.Context, () =>
                {
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

            private void AddShared(ScriptScope scope)
            {
                foreach (var descriptor in descriptors)
                {
                    descriptor.Describe(Add, scope);
                }

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

            private void AddData(FilterSchema dataSchema)
            {
                if (dataSchema.Fields == null)
                {
                    return;
                }

                foreach (var field in dataSchema.Fields)
                {
                    switch (field.Schema.Type)
                    {
                        case FilterSchemaType.Any:
                            AddAny(field.Path, field.Description);
                            break;
                        case FilterSchemaType.Boolean:
                            AddBoolean(field.Path, field.Description);
                            break;
                        case FilterSchemaType.DateTime:
                            AddString(field.Path, field.Description);
                            break;
                        case FilterSchemaType.GeoObject:
                            AddObject(field.Path, field.Description);
                            break;
                        case FilterSchemaType.Guid:
                            AddString(field.Path, field.Description);
                            break;
                        case FilterSchemaType.Number:
                            AddNumber(field.Path, field.Description);
                            break;
                        case FilterSchemaType.Object:
                            AddObject(field.Path, field.Description);
                            break;
                        case FilterSchemaType.ObjectArray:
                            AddArray(field.Path, field.Description);
                            break;
                        case FilterSchemaType.String:
                            AddString(field.Path, field.Description);
                            break;
                        case FilterSchemaType.StringArray:
                            AddArray(field.Path, field.Description);
                            break;
                    }
                }
            }

            private void AddAny(string? name, string? description)
            {
                Add(JsonType.Any, name, description);
            }

            private void AddArray(string? name, string? description)
            {
                Add(JsonType.Array, name, description);
            }

            private void AddBoolean(string? name, string? description)
            {
                Add(JsonType.Boolean, name, description);
            }

            private void AddObject(string? name, string? description)
            {
                Add(JsonType.Object, name, description);
            }

            private void AddNumber(string? name, string? description)
            {
                Add(JsonType.Number, name, description);
            }

            private void AddString(string? name, string? description)
            {
                Add(JsonType.String, name, description);
            }

            private void Add(JsonType type, string? name, string? description)
            {
                if (name != null)
                {
                    prefixes.Push(name);
                }

                if (prefixes.Count == 0)
                {
                    return;
                }

                var path = string.Join('.', prefixes.Reverse());

                result.Add(new ScriptingValue(path, type, description));

                if (name != null)
                {
                    prefixes.Pop();
                }
            }

            private void AddObject(string name, string description, Action inner)
            {
                Add(JsonType.Object, name, description);

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
}
