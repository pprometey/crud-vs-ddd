namespace Core.Common.Domain;

public interface IClock
{
    DateTime UtcNow { get; }
}
