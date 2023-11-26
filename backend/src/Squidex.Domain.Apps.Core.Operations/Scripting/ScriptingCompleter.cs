// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection.Internal;
using Squidex.Shared.Users;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed partial class ScriptingCompleter
{
    private readonly IEnumerable<IScriptDescriptor> descriptors;
    private static readonly FilterSchema DynamicData = new FilterSchema(FilterSchemaType.Object)
    {
        Fields = ReadonlyList.Create(
            new FilterField(new FilterSchema(FilterSchemaType.Object), "my-field"),
            new FilterField(FilterSchema.String, "my-field.iv"))
    };

    public ScriptingCompleter(IEnumerable<IScriptDescriptor> descriptors)
    {
        this.descriptors = descriptors;
    }

    public IReadOnlyList<ScriptingValue> Trigger(string type)
    {
        Guard.NotNull(type);

        switch (type)
        {
            case "AssetChanged":
                return AssetTrigger();
            case "Comment":
                return CommentTrigger();
            case "ContentChanged":
                return ContentTrigger(DynamicData);
            case "SchemaChanged":
                return SchemaTrigger();
            case "Usage":
                return UsageTrigger();
            default:
                return new List<ScriptingValue>();
        }
    }

    public IReadOnlyList<ScriptingValue> ContentScript(FilterSchema dataSchema)
    {
        Guard.NotNull(dataSchema);

        return new Process(descriptors, dataSchema.Flatten()).ContentScript();
    }

    public IReadOnlyList<ScriptingValue> ContentTrigger(FilterSchema dataSchema)
    {
        Guard.NotNull(dataSchema);

        return new Process(descriptors, dataSchema.Flatten()).ContentTrigger();
    }

    public IReadOnlyList<ScriptingValue> FieldRule(FilterSchema dataSchema)
    {
        Guard.NotNull(dataSchema);

        return new Process(descriptors, dataSchema.Flatten()).FieldRule();
    }

    public IReadOnlyList<ScriptingValue> PreviewUrl(FilterSchema dataSchema)
    {
        Guard.NotNull(dataSchema);

        return new Process(descriptors, dataSchema.Flatten()).PreviewUrl();
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

    private sealed partial class Process
    {
        private static readonly Regex RegexProperty = BuildPropertyRegex();
        private readonly Stack<string> prefixes = new Stack<string>();
        private readonly Dictionary<string, ScriptingValue> result = [];
        private readonly IEnumerable<IScriptDescriptor> descriptors;
        private readonly FilterSchema? dataSchema;

        public Process(IEnumerable<IScriptDescriptor> descriptors, FilterSchema? dataSchema = null)
        {
            this.descriptors = descriptors;
            this.dataSchema = dataSchema;
        }

        public IReadOnlyList<ScriptingValue> SchemaTrigger()
        {
            AddHelpers(ScriptScope.SchemaTrigger | ScriptScope.Async);

            AddObject("event", FieldDescriptions.Event, () =>
            {
                AddType(typeof(EnrichedSchemaEvent));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> CommentTrigger()
        {
            AddHelpers(ScriptScope.CommentTrigger | ScriptScope.Async);

            AddObject("event", FieldDescriptions.Event, () =>
            {
                AddType(typeof(EnrichedCommentEvent));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> UsageTrigger()
        {
            AddHelpers(ScriptScope.UsageTrigger | ScriptScope.Async);

            AddObject("event", FieldDescriptions.Event, () =>
            {
                AddType(typeof(EnrichedUsageExceededEvent));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> ContentScript()
        {
            AddHelpers(ScriptScope.ContentScript | ScriptScope.Transform | ScriptScope.Async);

            AddObject("ctx", FieldDescriptions.Context, () =>
            {
                AddType(typeof(ContentScriptVars));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> ContentTrigger()
        {
            AddHelpers(ScriptScope.ContentTrigger | ScriptScope.Async);

            AddObject("event", FieldDescriptions.Event, () =>
            {
                AddType(typeof(EnrichedContentEvent));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> AssetTrigger()
        {
            AddHelpers(ScriptScope.AssetTrigger | ScriptScope.Async);

            AddObject("event", FieldDescriptions.Event, () =>
            {
                AddType(typeof(EnrichedAssetEvent));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> AssetScript()
        {
            AddHelpers(ScriptScope.AssetScript | ScriptScope.Async);

            AddObject("ctx", FieldDescriptions.Event, () =>
            {
                AddType(typeof(AssetScriptVars));
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> PreviewUrl()
        {
            AddString("id",
                FieldDescriptions.EntityId);

            AddNumber("version",
                FieldDescriptions.EntityVersion);

            AddString("accessToken",
                FieldDescriptions.AccessToken);

            AddObject("data", FieldDescriptions.ContentData, () =>
            {
                AddData();
            });

            return Build();
        }

        public IReadOnlyList<ScriptingValue> FieldRule()
        {
            AddObject("user", FieldDescriptions.User, () =>
            {
                AddString("id",
                    FieldDescriptions.UserId);

                AddString("email",
                    FieldDescriptions.UserEmail);

                AddString("displayName",
                    FieldDescriptions.UserDisplayName);

                AddString("role",
                    FieldDescriptions.UserAppRole);
            });

            AddObject("data", FieldDescriptions.ContentData, () =>
            {
                AddData();
            });

            AddObject("itemData", FieldDescriptions.ItemData, () =>
            {
                AddAny("my-field", FieldDescriptions.ItemDataField);
            });

            return Build();
        }

        private IReadOnlyList<ScriptingValue> Build()
        {
            return result.Values.OrderBy(x => x.Path).ToList();
        }

        private void AddHelpers(ScriptScope scope)
        {
            foreach (var descriptor in descriptors)
            {
                descriptor.Describe(Add, scope);
            }
        }

        private void AddType(Type type)
        {
            foreach (var (name, description, propertyTypeOrNullable) in GetFields(type))
            {
                var propertyType = Nullable.GetUnderlyingType(propertyTypeOrNullable) ?? propertyTypeOrNullable;

                if (propertyType.IsEnum)
                {
                    var allowedValues = Enum.GetNames(propertyType);

                    Add(JsonType.String, name, description, allowedValues);
                }
                else if (
                    propertyType == typeof(string) ||
                    propertyType == typeof(DomainId) ||
                    propertyType == typeof(Instant) ||
                    propertyType == typeof(Status))
                {
                    AddString(name, description);
                }
                else if (propertyType == typeof(int) || propertyType == typeof(long))
                {
                    AddNumber(name, description);
                }
                else if (propertyType == typeof(bool))
                {
                    AddBoolean(name, description);
                }
                else if (typeof(MulticastDelegate).IsAssignableFrom(propertyType.BaseType))
                {
                    AddFunction(name, description);
                }
                else if (propertyType == typeof(NamedId<DomainId>))
                {
                    AddObject(name, description, () =>
                    {
                        AddString("id",
                            FieldDescriptions.NamedId);

                        AddString("name",
                            FieldDescriptions.NamedName);
                    });
                }
                else if (propertyType == typeof(RefToken))
                {
                    AddObject(name, description, () =>
                    {
                        AddString("identifier",
                            FieldDescriptions.ActorIdentifier);

                        AddString("type",
                            FieldDescriptions.ActorType);
                    });
                }
                else if (propertyType == typeof(IUser) || propertyType == typeof(ClaimsPrincipal))
                {
                    AddObject(name, description, () =>
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
                }
                else if (propertyType == typeof(ContentData) && dataSchema != null)
                {
                    AddObject(name, description, () =>
                    {
                        AddData();
                    });
                }
                else if (GetFields(propertyType).Any())
                {
                    AddObject(name, description, () =>
                    {
                        AddType(propertyType);
                    });
                }
                else if (IsDictionary(propertyType))
                {
                    AddObject(name, description, () =>
                    {
                        AddString("my-name",
                            FieldDescriptions.ObjectValue);
                    });
                }
                else if (IsCollection(propertyType))
                {
                    AddArray(name, description);
                }
            }
        }

        private static bool IsDictionary(Type propertyType)
        {
            return propertyType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
        }

        private static bool IsCollection(Type propertyType)
        {
            return propertyType.GetInterfaces().Contains(typeof(IEnumerable));
        }

        private static IEnumerable<(string Name, string Description, Type Type)> GetFields(Type type)
        {
            foreach (var property in type.GetPublicProperties())
            {
                var descriptionKey = property.GetCustomAttribute<FieldDescriptionAttribute>()?.Name;

                if (descriptionKey == null)
                {
                    continue;
                }

                var description = FieldDescriptions.ResourceManager.GetString(descriptionKey, CultureInfo.InvariantCulture)!;

                yield return (property.Name.ToCamelCase(), description, property.PropertyType);
            }
        }

        private void AddData()
        {
            if (dataSchema?.Fields == null)
            {
                return;
            }

            foreach (var field in dataSchema.Fields)
            {
                JsonType type = ConvertType(field);

                Add(type, field.Path, field.Description, field.Schema.AllowedValues);
            }

            static JsonType ConvertType(FilterField field)
            {
                switch (field.Schema.Type)
                {
                    case FilterSchemaType.Any:
                        return JsonType.Any;
                    case FilterSchemaType.Boolean:
                        return JsonType.Boolean;
                    case FilterSchemaType.DateTime:
                        return JsonType.String;
                    case FilterSchemaType.GeoObject:
                        return JsonType.Object;
                    case FilterSchemaType.Guid:
                        return JsonType.String;
                    case FilterSchemaType.Number:
                        return JsonType.Number;
                    case FilterSchemaType.Object:
                        return JsonType.Object;
                    case FilterSchemaType.ObjectArray:
                        return JsonType.Array;
                    case FilterSchemaType.String:
                        return JsonType.String;
                    case FilterSchemaType.StringArray:
                        return JsonType.Array;
                    default:
                        return JsonType.String;
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

        private void AddNumber(string? name, string? description)
        {
            Add(JsonType.Number, name, description);
        }

        private void AddFunction(string? name, string? description)
        {
            Add(JsonType.Function, name, description);
        }

        private void AddString(string? name, string? description)
        {
            Add(JsonType.String, name, description);
        }

        private void Add(JsonType type, string? name, string? description, string[]? allowedValues = null, string? decprecationReason = null)
        {
            var parts = name?.Split('.') ?? [];

            foreach (var part in parts)
            {
                PushPrefix(part);
            }

            if (prefixes.Count == 0)
            {
                return;
            }

            var path = string.Concat(prefixes.Reverse());

            result[path] = new ScriptingValue(path, type)
            {
                AllowedValues = allowedValues,
                Description = description,
                DeprecationReason = decprecationReason
            };

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
            else if (RegexProperty.IsMatch(name))
            {
                prefixes.Push($".{name}");
            }
            else
            {
                prefixes.Push($"['{name}']");
            }
        }

        [GeneratedRegex("^(?!\\d)[\\w$]+$", RegexOptions.Compiled)]
        private static partial Regex BuildPropertyRegex();
    }
}
