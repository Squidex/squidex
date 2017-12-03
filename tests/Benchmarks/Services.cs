// ==========================================================================
//  Services.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Benchmarks.Tests.TestData;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Benchmarks
{
    public static class Services
    {
        public static IServiceProvider Create()
        {
            var services = new ServiceCollection();

            services.AddSingleton(CreateTypeNameRegistry());

            services.AddSingleton<EventDataFormatter>();
            services.AddSingleton<FieldRegistry>();

            services.AddTransient<MyAppState>();

            services.AddSingleton<IMongoClient>(
                new MongoClient("mongodb://localhost"));

            services.AddSingleton<ISemanticLog>(
                new SemanticLog(new ILogChannel[0], new ILogAppender[0], () => new JsonLogWriter()));

            services.AddSingleton<IMemoryCache>(
                new MemoryCache(Options.Create(new MemoryCacheOptions())));

            services.AddSingleton<IPubSub,
                InMemoryPubSub>();

            services.AddSingleton<IEventNotifier,
                DefaultEventNotifier>();

            services.AddSingleton<IEventStore,
                MongoEventStore>();

            services.AddSingleton<IStateStore,
                MongoSnapshotStore>();

            services.AddSingleton<IStateFactory,
                StateFactory>();

            services.AddSingleton<JsonSerializer>(c =>
                JsonSerializer.Create(c.GetRequiredService<JsonSerializerSettings>()));

            services.AddSingleton<JsonSerializerSettings>(c =>
                CreateJsonSerializerSettings(c.GetRequiredService<TypeNameRegistry>(), c.GetRequiredService<FieldRegistry>()));

            services.AddSingleton(c =>
                c.GetRequiredService<IMongoClient>().GetDatabase(Guid.NewGuid().ToString()));

            return services.BuildServiceProvider();
        }

        public static void Cleanup(this IServiceProvider services)
        {
            var mongoClient = services.GetRequiredService<IMongoClient>();
            var mongoDatabase = services.GetRequiredService<IMongoDatabase>();

            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);

            if (services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private static TypeNameRegistry CreateTypeNameRegistry()
        {
            var result = new TypeNameRegistry();

            result.Map(typeof(MyEvent));

            return result;
        }

        private static JsonSerializerSettings CreateJsonSerializerSettings(TypeNameRegistry typeNameRegistry, FieldRegistry fieldRegistry)
        {
            var settings = new JsonSerializerSettings();

            settings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);

            settings.ContractResolver = new ConverterContractResolver(
                new AppClientsConverter(),
                new AppContributorsConverter(),
                new ClaimsPrincipalConverter(),
                new InstantConverter(),
                new LanguageConverter(),
                new LanguagesConfigConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new PropertiesBagConverter<EnvelopeHeaders>(),
                new PropertiesBagConverter<PropertiesBag>(),
                new RefTokenConverter(),
                new RuleConverter(),
                new SchemaConverter(fieldRegistry),
                new StringEnumConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = TypeNameHandling.Auto;

            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            BsonJsonConvention.Register(JsonSerializer.Create(settings));

            return settings;
        }
    }
}
