using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using AnimeDl.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Utils.JsonConverters;

internal class NameValueCollectionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(NameValueCollection);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var dict = token.ToObject<Dictionary<string, string>>()!;

        var nvc = new NameValueCollection(dict.Count);
        foreach (var k in dict)
            nvc.Add(k.Key, k.Value);

        return nvc;
    }

    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var nvc = ((NameValueCollection)value!).ToDictionary();
        var json = JsonConvert.SerializeObject(nvc);

        writer.WriteRaw(json);
    }
}