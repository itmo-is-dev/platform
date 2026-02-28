namespace Itmo.Dev.Platform.Testing.Behavioural.Text;

public interface ITextWriter
{
    void Write(string value);

    void WriteLine(string value);

    ITextWriterScope Indent();
}
