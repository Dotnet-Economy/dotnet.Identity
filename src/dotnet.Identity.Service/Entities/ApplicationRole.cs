using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace dotnet.Identity.Service.Entities
{
    [CollectionName("Roles")]
    public class AppplicationRole : MongoIdentityRole<Guid>
    {
    }
}