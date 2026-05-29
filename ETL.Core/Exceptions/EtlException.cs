namespace ETL.Core.Exceptions;

public class EtlException : Exception
{
    public string ProcessName { get; }

    public EtlException(string processName, string message, Exception? inner = null)
        : base(message, inner)
    {
        ProcessName = processName;
    }
}
