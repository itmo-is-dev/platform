namespace Itmo.Dev.Platform.MessagePersistence.Tools;

public static class MessagePersistenceConstants
{
    public const string DefaultPublisherName = "Default";

    public static class Tracing
    {
        public const string SpanName = "Message Persistence";
        public const string TraceParentHeader = "traceparent";

        public const string MessageIdTag = "persisted_message_id";
        public const string MessageNameTag = "persisted_message_name";
        public const string MessageBufferingStepTag = "persisted_message_buffering_step";
        public const string MessageStateTag = "persisted_message_state";
        public const string MessageRetryCountTag = "persisted_message_retry_count";
    }
}
