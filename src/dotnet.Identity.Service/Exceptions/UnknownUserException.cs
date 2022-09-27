using System;

namespace dotnet.Identity.Service.Exceptions
{
    internal class UnknownUserException : Exception
    {
        public UnknownUserException(Guid userId) : base($"Unknown User '{userId}'")
        {
            this.UserId = userId;
        }

        public Guid UserId;

    }
}