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
using Newtonsoft.Json.Serialization;
using Squidex.Core.Schemas;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.EventStore;
using Squidex.Infrastructure.Json;

namespace Squidex.Configurations.Domain
{
    public static class Serializers
    {
        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings)
        {
            settings.SerializationBinder = new TypeNameSerializationBinder();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new PropertiesBagConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.TypeNameHandling = TypeNameHandling.Auto;

            return settings;
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return ConfigureJson(new JsonSerializerSettings());
        }

        private static JsonSerializer CreateSerializer(JsonSerializerSettings settings)
        {
            return JsonSerializer.Create(settings);
        }

        public static IServiceCollection AddMyEventFormatter(this IServiceCollection services)
        {
            TypeNameRegistry.Map(typeof(Schema).GetTypeInfo().Assembly);
            TypeNameRegistry.Map(typeof(SchemaCreated).GetTypeInfo().Assembly);

            services.AddSingleton(t => CreateSettings());
            services.AddSingleton(t => CreateSerializer(t.GetRequiredService<JsonSerializerSettings>()));
            services.AddSingleton<EventStoreFormatter>();

            return services;
        }

        public static IMvcBuilder AddMySerializers(this IMvcBuilder mvc)
        {
            mvc.AddJsonOptions(options =>
            {
                ConfigureJson(options.SerializerSettings);
            });

            return mvc;
        }
    }
}
