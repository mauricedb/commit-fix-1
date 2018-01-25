using Newtonsoft.Json.Linq;

public static class CommitEventExtensions
{
    public static string EventType(this JToken @event)
    {
        return @event.Body()["$type"].ToString();
    }

    public static JToken Body(this JToken @event)
    {
        return @event["Body"];
    }

    public static JToken Headers(this JToken @event)
    {
        return @event["Headers"];
    }

    public static JToken SetHeaders(this JToken @event, JToken headers)
    {
        @event["Headers"] = headers;

        return @event;
    }
}