// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
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
            Guard.NotNull(dataSchema);

            return new Process(descriptors).ContentScript(dataSchema.Flatten());
        }

        public IReadOnlyList<ScriptingValue> ContentTrigger(FilterSchema dataSchema)
        {
            Guard.NotNull(dataSchema);

            return new Process(descriptors).ContentTrigger(dataSchema.Flatten());
        }

        public IReadOnlyList<ScriptingValue> AssetScript()
        {
            return new Process(descriptors).AssetScript();
        }

        public IReadOnlyList<ScriptingValue> AssetTrigger()
        {
            return new Process(descriptors).AssetTrigger();
        }

        public IReadOnlyList<ScriptingValue> CommentTrigger()
        {
            return new Process(descriptors).CommentTrigger();
        }

        public IReadOnlyList<ScriptingValue> SchemaTrigger()
        {
            return new Process(descriptors).SchemaTrigger();
        }

        public IReadOnlyList<ScriptingValue> UsageTrigger()
        {
            return new Process(descriptors).UsageTrigger();
        }

        private sealed class Process
        {
            private static readonly Regex PropertyRegex = new Regex(@"^(?!\d)[\w$]+$", RegexOptions.Compiled);
            private readonly Stack<string> prefixes = new Stack<string>();
            private readonly Dictionary<string, ScriptingValue> result = new Dictionary<string, ScriptingValue>();
            private readonly IEnumerable<IScriptDescriptor> descriptors;

            public Process(IEnumerable<IScriptDescriptor> descriptors)
            {
                this.descriptors = descriptors;
            }

            private IReadOnlyList<ScriptingValue> Build()
            {
                return result.Values.OrderBy(x => x.Path).ToList();
            }

            public IReadOnlyList<ScriptingValue> SchemaTrigger()
            {
                var scope = ScriptScope.SchemaTrigger;

                AddHelpers(scope);

                AddObject("event", FieldDescriptions.Context, () =>
                {
                    AddEvent();

                    AddString("schemaId.id",
                        FieldDescriptions.ContentSchemaId);

                    AddString("schemaId.name",
                        FieldDescriptions.ContentSchemaName);
                });

                return Build();
            }

            public IReadOnlyList<ScriptingValue> CommentTrigger()
            {
                var scope = ScriptScope.ContentTrigger;

                AddHelpers(scope);

                AddObject("event", FieldDescriptions.Context, () =>
                {
                    AddEvent();

                    AddUser("mentionedUser",
                        FieldDescriptions.CommentMentionedUser);

                    AddString("text",
                        FieldDescriptions.CommentText);

                    AddString("url",
                        FieldDescriptions.CommentUrl);
                });

                return Build();
            }

            public IReadOnlyList<ScriptingValue> UsageTrigger()
            {
                var scope = ScriptScope.ContentTrigger;

                AddHelpers(scope);

                AddObject("event", FieldDescriptions.Context, () =>
                {
                    AddEvent();

                    AddNumber("callsCurrent",
                        FieldDescriptions.UsageCallsCurrent);

                    AddNumber("callsLimit",
                        FieldDescriptions.UsageCallsLimit);
                });

                return Build();
            }

            public IReadOnlyList<ScriptingValue> ContentScript(FilterSchema dataSchema)
            {
                var scope = ScriptScope.ContentScript | ScriptScope.Transform | ScriptScope.Async;

                AddHelpers(scope);

                AddObject("ctx", FieldDescriptions.Context, () =>
                {
                    AddScript();

                    AddString("contentId",
                        FieldDescriptions.EntityId);

                    AddString("permanent",
                        FieldDescriptions.EntityRequestDeletePermanent);

                    AddString("schemaId",
                        FieldDescriptions.ContentSchemaId);

                    AddString("schemaName",
                        FieldDescriptions.ContentSchemaName);

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

                return Build();
            }

            public IReadOnlyList<ScriptingValue> ContentTrigger(FilterSchema dataSchema)
            {
                var scope = ScriptScope.ContentTrigger;

                AddHelpers(scope);

                AddObject("event", FieldDescriptions.Context, () =>
                {
                    AddEntity().AddEvent().AddContent(dataSchema);

                    AddObject("dataOld", FieldDescriptions.ContentDataOld, () =>
                    {
                        AddData(dataSchema);
                    });
                });

                return Build();
            }

            public IReadOnlyList<ScriptingValue> AssetTrigger()
            {
                var scope = ScriptScope.AssetTrigger;

                AddHelpers(scope);

                AddObject("event", FieldDescriptions.Context, () =>
                {
                    AddAsset().AddEntity().AddEvent();

                    AddString("assetType",
                        FieldDescriptions.AssetType);

                    AddNumber("fileVersion",
                        FieldDescriptions.AssetFileVersion);

                    AddNumber("isImage",
                        FieldDescriptions.AssetIsImage);

                    AddNumber("pixelHeight",
                        FieldDescriptions.AssetPixelHeight);

                    AddNumber("pixelWidth",
                        FieldDescriptions.AssetPixelWidth);
                });

                return Build();
            }

            public IReadOnlyList<ScriptingValue> AssetScript()
            {
                var scope = ScriptScope.AssetScript | ScriptScope.Async;

                AddHelpers(scope);

                AddObject("ctx", FieldDescriptions.Event, () =>
                {
                    AddScript();

                    AddString("assetId",
                        FieldDescriptions.EntityId);

                    AddObject("command", FieldDescriptions.Command, () =>
                    {
                        AddAsset();

                        AddString("parentPath",
                            FieldDescriptions.AssetParentPath);

                        AddString("permanent",
                            FieldDescriptions.EntityRequestDeletePermanent);

                        AddArray("tags",
                            FieldDescriptions.AssetTags);
                    });

                    AddObject("asset", FieldDescriptions.Asset, () =>
                    {
                        AddAsset();

                        AddNumber("fileVersion",
                            FieldDescriptions.AssetFileVersion);

                        AddArray("tags",
                            FieldDescriptions.AssetTags);
                    });
                });

                return Build();
            }

            private Process AddContent(FilterSchema dataSchema)
            {
                AddString("newStatus",
                    FieldDescriptions.ContentStatusOld);

                AddString("status",
                    FieldDescriptions.ContentStatus);

                AddString("schemaId.id",
                    FieldDescriptions.ContentSchemaId);

                AddString("schemaId.name",
                    FieldDescriptions.ContentSchemaName);

                AddObject("data", FieldDescriptions.ContentData, () =>
                {
                    AddData(dataSchema);
                });

                return this;
            }

            private Process AddAsset()
            {
                AddString("fileHash",
                    FieldDescriptions.AssetFileHash);

                AddString("fileName",
                    FieldDescriptions.AssetFileName);

                AddNumber("fileSize",
                    FieldDescriptions.AssetFileSize);

                AddString("mimeType",
                    FieldDescriptions.AssetMimeType);

                AddBoolean("isProtected",
                    FieldDescriptions.AssetIsProtected);

                AddString("parentId",
                    FieldDescriptions.AssetParentId);

                AddString("slug",
                    FieldDescriptions.AssetSlug);

                AddObject("metadata", FieldDescriptions.AssetMetadata, () =>
                {
                    AddArray("name",
                        FieldDescriptions.AssetMetadataValue);
                });

                return this;
            }

            private Process AddEntity()
            {
                AddString("id",
                    FieldDescriptions.EntityId);

                AddString("appId.id",
                    FieldDescriptions.AppId);

                AddString("appId.name",
                    FieldDescriptions.AppName);

                AddString("created",
                    FieldDescriptions.EntityCreated);

                AddString("createdBy",
                    FieldDescriptions.EntityCreatedBy);

                AddString("lastModified",
                    FieldDescriptions.EntityLastModified);

                AddString("lastModifiedBy",
                    FieldDescriptions.EntityLastModifiedBy);

                AddString("version",
                    FieldDescriptions.EntityVersion);

                return this;
            }

            private Process AddScript()
            {
                AddString("appId",
                        FieldDescriptions.AppId);

                AddString("appName",
                    FieldDescriptions.AppName);

                AddString("operation",
                    FieldDescriptions.Operation);

                return AddUser();
            }

            private Process AddEvent()
            {
                AddString("appId.id",
                    FieldDescriptions.AppId);

                AddString("appId.name",
                    FieldDescriptions.AppName);

                AddString("name",
                    FieldDescriptions.EventName);

                AddString("timestamp",
                    FieldDescriptions.EventTimestamp);

                AddString("type",
                    FieldDescriptions.EventType);

                AddString("version",
                    FieldDescriptions.EntityVersion);

                return AddActor().AddUser();
            }

            private Process AddActor()
            {
                AddObject("actor", FieldDescriptions.Actor, () =>
                {
                    AddString("identifier",
                        FieldDescriptions.ActorIdentifier);

                    AddString("type",
                        FieldDescriptions.ActorType);
                });

                return this;
            }

            private Process AddUser(string? name = null, string? description = null)
            {
                AddObject(name ?? "user", description ?? FieldDescriptions.User, () =>
                {
                    AddString("id",
                        FieldDescriptions.UserId);

                    AddString("email",
                        FieldDescriptions.UserEmail);

                    AddBoolean("isClient",
                        FieldDescriptions.UserIsClient);

                    AddBoolean("isUser",
                        FieldDescriptions.UserIsUser);

                    AddObject("claims", FieldDescriptions.UserClaims, () =>
                    {
                        AddArray("name",
                            FieldDescriptions.UsersClaimsValue);
                    });
                });

                return this;
            }

            private void AddHelpers(ScriptScope scope)
            {
                foreach (var descriptor in descriptors)
                {
                    descriptor.Describe(Add, scope);
                }
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
                var parts = name?.Split('.') ?? Array.Empty<string>();

                foreach (var part in parts)
                {
                    PushPrefix(part);
                }

                if (prefixes.Count == 0)
                {
                    return;
                }

                var path = string.Concat(prefixes.Reverse());

                result[path] = new ScriptingValue(path, type, description);

                for (int i = 0; i < parts.Length; i++)
                {
                    prefixes.Pop();
                }
            }

            private void AddObject(string name, string description, Action inner)
            {
                Add(JsonType.Object, name, description);

                var parts = name.Split('.');

                foreach (var part in parts)
                {
                    PushPrefix(part);
                }

                inner();

                for (int i = 0; i < parts.Length; i++)
                {
                    prefixes.Pop();
                }
            }

            private void PushPrefix(string name)
            {
                if (prefixes.Count == 0)
                {
                    prefixes.Push(name);
                }
                else if (PropertyRegex.IsMatch(name))
                {
                    prefixes.Push($".{name}");
                }
                else
                {
                    prefixes.Push($"['{name}']");
                }
            }
        }
    }
}
