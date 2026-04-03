namespace Core.Common.Domain;

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string errorCode, string message, Exception inner) : base(message, inner)
    {
        ErrorCode = errorCode;
    }

    // Kept for backwards compatibility — prefer the overload with errorCode
    public DomainException(string message) : base(message)
    {
        ErrorCode = "domain.unspecified";
    }

    public DomainException(string message, Exception inner) : base(message, inner)
    {
        ErrorCode = "domain.unspecified";
    }
}
