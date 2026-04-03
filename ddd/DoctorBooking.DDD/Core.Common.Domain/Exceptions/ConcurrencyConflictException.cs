using System.Resources;

namespace Core.Common.Domain;

public sealed class ConcurrencyConflictException : Exception
{
    private static readonly ResourceManager Resources =
        new("Core.Common.Domain.CommonMessages", typeof(ConcurrencyConflictException).Assembly);

    public const string ErrorCode = "concurrency.conflict";

    public ConcurrencyConflictException()
        : base(FeatureMessages.Msg(Resources, ErrorCode)) { }

    public ConcurrencyConflictException(Exception inner)
        : base(FeatureMessages.Msg(Resources, ErrorCode), inner) { }
}
