using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class EndSessionResponseGenerator
    {
        public SignOutMessage CreateSignoutMessage(ValidatedEndSessionRequest request)
        {
            var message = new SignOutMessage();
            
            if (request.Client != null)
            {
                message.ClientId = request.Client.ClientId;

                if (request.PostLogOutUri != null)
                {
                    message.ReturnUrl = request.PostLogOutUri.AbsoluteUri;
                }
                else
                {
                    if (request.Client.PostLogoutRedirectUris.Any())
                    {
                        message.ReturnUrl = request.Client.PostLogoutRedirectUris.First().AbsoluteUri;
                    }
                }

                if (request.State.IsPresent())
                {
                    if (message.ReturnUrl.IsPresent())
                    {
                        message.ReturnUrl = message.ReturnUrl.AddQueryString("state=" + request.State);
                    }
                }
            }

            return message;
        }
    }
}
