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
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Infrastructure.Json;

namespace PinkParrot.Configurations
{
    public static class Serializers
    {
        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings)
        {
            settings.Binder = new TypeNameSerializationBinder().Map(typeof(ModelSchema).GetTypeInfo().Assembly);
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new PropertiesBagConverter());
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.TypeNameHandling = TypeNameHandling.Auto;

            return settings;
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return ConfigureJson(new JsonSerializerSettings());
        }

        public static void AddEventFormatter(this IServiceCollection services)
        {
            var fieldFactory =
                new ModelFieldFactory()
                    .AddFactory<NumberFieldProperties>(id => new NumberField(id));

            services.AddSingleton(t => CreateSettings());
            services.AddSingleton(fieldFactory);
            services.AddSingleton<EventStoreParser>();
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
