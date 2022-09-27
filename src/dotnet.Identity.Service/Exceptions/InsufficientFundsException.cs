using System;
using System.Runtime.Serialization;

namespace dotnet.Identity.Service.Exceptions
{
    internal class InsufficientFundsException : Exception
    {

        public InsufficientFundsException(Guid userId, decimal okuboToComot) : base($"Not enough Okubo to comot {okuboToComot} from {userId}")
        {
            this.UserId = userId;
            this.OkuboToComot = okuboToComot;
        }

        public Guid UserId { get; }
        public decimal OkuboToComot { get; }

    }
}