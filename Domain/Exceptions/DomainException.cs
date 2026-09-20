namespace EcoBridge.Domain.Exceptions;

/// <summary>
/// Base exception for all domain rule violations.
/// Carries only a stable error code — messages are localized on the client.
/// The nested type communicates the error category for HTTP status mapping.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code) : base(code)
    {
        Code = code;
    }

    /// <summary>Invalid input, business rule violation, or expired state.</summary>
    public sealed class ValidationException : DomainException
    {
        public ValidationException(string code) : base(code) { }
    }

    /// <summary>Requested entity does not exist.</summary>
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string code) : base(code) { }
    }

    /// <summary>State conflict — already taken, already accepted, concurrency clash.</summary>
    public sealed class ConflictException : DomainException
    {
        public ConflictException(string code) : base(code) { }
    }

    /// <summary>Authenticated but not allowed (role, ownership, availability).</summary>
    public sealed class ForbiddenException : DomainException
    {
        public ForbiddenException(string code) : base(code) { }
    }

    /// <summary>Not authenticated or token invalid/expired/revoked.</summary>
    public sealed class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string code) : base(code) { }
    }

    /// <summary>Unhandled/unspecified domain failure.</summary>
    public sealed class FailureException : DomainException
    {
        public FailureException(string code) : base(code) { }
    }
}