// ==========================================================================
//  Serializers.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json;

namespace Squidex.Config.Domain
{
    public static class Serializers
    {
        private static readonly TypeNameRegistry TypeNameRegistry = new TypeNameRegistry();

        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings, TypeNameHandling typeNameHandling)
        {
            settings.SerializationBinder = new TypeNameSerializationBinder(TypeNameRegistry);

            settings.ContractResolver = new ConverterContractResolver(
                new InstantConverter(),
                new LanguageConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new PropertiesBagConverter(),
                new RefTokenConverter(),
                new StringEnumConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;

            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            return settings;
        }

        static Serializers()
        {
            TypeNameRegistry.Map(typeof(SquidexEvent).GetTypeInfo().Assembly);
            TypeNameRegistry.Map(typeof(NoopEvent).GetTypeInfo().Assembly);
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return ConfigureJson(new JsonSerializerSettings(), TypeNameHandling.Auto);
        }

        private static JsonSerializer CreateSerializer(JsonSerializerSettings settings)
        {
            return JsonSerializer.Create(settings);
        }

        public static IServiceCollection AddMyEventFormatter(this IServiceCollection services)
        {
            services.AddSingleton(t => TypeNameRegistry);
            services.AddSingleton(t => CreateSettings());
            services.AddSingleton(t => CreateSerializer(t.GetRequiredService<JsonSerializerSettings>()));

            return services;
        }

        public static IMvcBuilder AddMySerializers(this IMvcBuilder mvc)
        {
            mvc.AddJsonOptions(options =>
            {
                ConfigureJson(options.SerializerSettings, TypeNameHandling.None);
            });

            return mvc;
        }
    }
}
