using System;
using System.Collections.Generic;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace dotnet.Identity.Service.Entities
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public decimal Okubo { get; set; }
        public HashSet<Guid> MessageIds { get; set; } = new();
    }

}