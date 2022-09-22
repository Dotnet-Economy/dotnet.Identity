using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace dotnet.Identity.Service.Settings
{
    public class IdentityServerSettings
    {
        public IReadOnlyCollection<ApiScope> ApiScopes { get; set; } = Array.Empty<ApiScope>();
        public IReadOnlyCollection<Client> Clients { get; set; } = Array.Empty<Client>();
        public IReadOnlyCollection<IdentityResource> IdentityResources => new IdentityResource[] {
            new IdentityResources.OpenId()
        };
    }
}