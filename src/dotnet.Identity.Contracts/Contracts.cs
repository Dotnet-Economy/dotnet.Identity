using System;

namespace dotnet.Identity.Contracts
{
    public record ComotOkubo(Guid UserId, decimal Okubo, Guid CorrelationId);
    public record OkuboDonComot(Guid CorrelationId);
    public record UserUpdated(Guid UserId, string Email, decimal NewTotalOkubo);
}
