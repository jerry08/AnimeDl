using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Utils.JsonConverters;

internal class WebHeaderCollectionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(WebHeaderCollection);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        var gs = token.ToString();

        var gg = JArray.Parse(token.ToString());
        var hh = gg.ToObject<WebHeaderCollection>();

        return token;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException("There is no writing implemantation");
    }
}