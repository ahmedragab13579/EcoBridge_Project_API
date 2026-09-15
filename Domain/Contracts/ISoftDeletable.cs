using System;

namespace EcoBridge.Domain.Contracts;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
