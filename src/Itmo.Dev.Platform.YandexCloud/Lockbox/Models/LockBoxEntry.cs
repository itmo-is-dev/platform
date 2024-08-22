using System.Text;

namespace Itmo.Dev.Platform.YandexCloud.Lockbox.Models;

internal sealed record LockBoxEntry(string Key, string TextValue, string BinaryValue)
{
    public string Value => string.IsNullOrWhiteSpace(TextValue)
        ? Encoding.UTF8.GetString(Convert.FromBase64String(BinaryValue))
        : TextValue;

    public override string ToString()
    {
        return string.Join(" = ", Key, Value);
    }
}