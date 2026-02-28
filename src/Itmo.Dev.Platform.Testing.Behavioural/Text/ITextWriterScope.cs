namespace Itmo.Dev.Platform.Testing.Behavioural.Text;

public interface ITextWriterScope : IDisposable
{
    ITextWriter Writer { get; }
}
