// ==========================================================================
//  JsonState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Json.Orleans
{
    public sealed class JsonState<TValue> where TValue : class, new()
    {
        private JsonSerializer serializer = JsonSerializer.CreateDefault();

        private TValue value;

        [JsonProperty]
        public JToken Json { get; set; }

        [JsonIgnore]
        public TValue Value
        {
            get
            {
                if (value == null)
                {
                    value = ReadState();
                }

                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public void SetSerializer(JsonSerializer serializer)
        {
            if (serializer != null)
            {
                this.serializer = serializer;
            }
        }

        public JsonState<TValue> Update(Action<TValue> updater)
        {
            var value = ReadState();

            updater(value);

            var json = JObject.FromObject(value, serializer);

            return new JsonState<TValue> { serializer = serializer, Json = json, Value = value };
        }

        private TValue ReadState()
        {
            if (Json != null)
            {
                return Json.ToObject<TValue>(serializer);
            }
            else
            {
                return new TValue();
            }
        }
    }
}
