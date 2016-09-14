// ==========================================================================
//  Serializers.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PinkParrot.Core.Schemas;
using PinkParrot.Events.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Infrastructure.Json;

namespace PinkParrot.Configurations
{
    public static class Serializers
    {
        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings)
        {
            settings.Binder = new TypeNameSerializationBinder();
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

        public static void AddEventFormatter(this IServiceCollection services)
        {
            TypeNameRegistry.Map(typeof(Schema).GetTypeInfo().Assembly);
            TypeNameRegistry.Map(typeof(SchemaCreated).GetTypeInfo().Assembly);

            services.AddSingleton(t => CreateSettings());
            services.AddSingleton(t => CreateSerializer(t.GetRequiredService<JsonSerializerSettings>()));
            services.AddSingleton<EventStoreFormatter>();
        }

        public static void AddAppSerializers(this IMvcBuilder mvc)
        {
            mvc.AddJsonOptions(options =>
            {
                ConfigureJson(options.SerializerSettings);
            });
        }
    }
}
