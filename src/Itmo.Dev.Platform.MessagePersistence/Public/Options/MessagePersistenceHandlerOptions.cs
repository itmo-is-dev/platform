using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Options;

[OptionsType]
public class MessagePersistenceHandlerOptions
{
    [DefaultValue(MessageHandleResultKind.Success)]
    [Description("Handle result that will be used when handling message, if SetResult was not explicitly called")]
    public MessageHandleResultKind DefaultHandleResult { get; set; }

    [Range(minimum: 0, maximum: int.MaxValue)]
    public int? RetryCount { get; set; }
}
