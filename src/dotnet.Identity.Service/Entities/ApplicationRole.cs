using System;
using AspNetCore.Identity.MongoDbCore.Models;

namespace dotnet.Identity.Service.Entities
{
    public class AppplicationRole : MongoIdentityRole<Guid>
    {
        public int MyProperty { get; set; }
    }
}