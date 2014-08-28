/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
  public class LogOutMessage
  {
    static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

    public string ReturnUrl { get; set; }

    // internal use
    public DateTime ValidTo { get; set; }

    public string Protect(int ttl, IDataProtector protector)
    {
      ValidTo = DateTime.UtcNow.AddSeconds(ttl);

      var settings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
      };

      var json = JsonConvert.SerializeObject(this, settings);
      Logger.DebugFormat("Protecting logout message: {0}", json);

      return protector.Protect(json, "logoutmessage");
    }

    public static LogOutMessage Unprotect(string data, IDataProtector protector)
    {
      var json = protector.Unprotect(data, "logoutmessage");
      var message = JsonConvert.DeserializeObject<LogOutMessage>(json);

      if (DateTime.UtcNow > message.ValidTo)
      {
        throw new Exception("LogOutMessage expired.");
      }

      return message;
    }
  }
}