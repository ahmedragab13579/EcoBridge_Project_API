using System;

namespace EcoBridge.Domain.Contracts;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get;}
    public void SoftDelete();
}
