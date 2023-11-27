using System.Diagnostics;
using System.Text;

namespace Itmo.Dev.Platform.Common.Tools;

internal class FormattedStackTrace
{
    private readonly StackTrace _stackTrace;
    private readonly int _indentation;

    public FormattedStackTrace(StackTrace stackTrace, int indentation)
    {
        _stackTrace = stackTrace;
        _indentation = indentation;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (StackFrame frame in _stackTrace.GetFrames())
        {
            builder.Append(new string('\t', _indentation));
            builder.Append(frame.GetFileName());
            builder.Append('.');
            builder.Append(frame.GetMethod()?.Name);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}