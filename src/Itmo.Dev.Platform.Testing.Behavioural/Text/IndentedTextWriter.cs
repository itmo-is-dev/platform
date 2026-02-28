using System.Text;

namespace Itmo.Dev.Platform.Testing.Behavioural.Text;

public sealed class IndentedTextWriter : ITextWriter
{
    private readonly StringBuilder _builder = new();
    private bool _shouldIndent = true;
    private int _indentation;

    public void Write(string value)
    {
        WriteIndentation();
        _builder.Append(value);
        _shouldIndent = false;
    }

    public void WriteLine(string value)
    {
        WriteIndentation();
        _builder.AppendLine(value);
        _shouldIndent = true;
    }

    public ITextWriterScope Indent()
    {
        _indentation++;
        return new TextWriterScope(this);
    }

    public override string ToString() => _builder.ToString();

    private void WriteIndentation()
    {
        if (_shouldIndent)
        {
            _builder.Append(new string(' ', _indentation));
        }
    }

    private class TextWriterScope(IndentedTextWriter writer) : ITextWriterScope
    {
        public ITextWriter Writer => writer;

        public void Dispose()
        {
            writer._indentation--;
        }
    }
}
