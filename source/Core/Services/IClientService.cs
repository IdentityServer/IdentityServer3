/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface IClientService
    {
        Task<Client> FindClientByIdAsync(string clientId);
    }
}