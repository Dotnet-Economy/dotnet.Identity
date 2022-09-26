using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace dotnet.Identity.Service.Settings
{
    public class IdentityServerSettings
    {
        public IReadOnlyCollection<ApiScope> ApiScopes { get; init; }
        public IReadOnlyCollection<ApiResource> ApiResources { get; init; } // = Array.Empty<ApiScope>();
        public IReadOnlyCollection<Client> Clients { get; init; } // = Array.Empty<Client>();
        public IReadOnlyCollection<IdentityResource> IdentityResources =>
            new IdentityResource[] {
                new IdentityResources.OpenId(),
                // Adds ability to request user info(email, name, age, birthdate, e.t.c.) in tokens
                new IdentityResources.Profile(),
                new IdentityResource("roles",new[]{"role"})
            };
    }
}