namespace Itmo.Dev.Platform.Testing.Behavioural.Tools;

public sealed record MockKafkaMessage<TKey, TValue>(TKey Key, TValue Value)
{
    public bool Observed { get; private set; }

    public void Observe()
    {
        Observed = true;
    }
}
