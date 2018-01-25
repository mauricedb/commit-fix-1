using Newtonsoft.Json.Linq;

public static class CommitExtensions
{
    public static JArray AsJArray(this string json)
    {
        return JArray.Parse(json);
    }

    public static JObject AsJObject(this string json)
    {
        return JObject.Parse(json);
    }
}