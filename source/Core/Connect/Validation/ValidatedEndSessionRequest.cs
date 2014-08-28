/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
  public class ValidatedEndSessionRequest
  {
    public NameValueCollection Raw { get; set; }
    public string IdTokenHint { get; set; }
    public Uri PostLogoutRedirectUri { get; set; }
    public string State { get; set; }

    // Parsed Id Token
    public IEnumerable<Claim> Claims { get; set; }
  }
}