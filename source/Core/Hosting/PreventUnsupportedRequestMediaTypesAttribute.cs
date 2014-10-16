using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Resources;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class PreventUnsupportedRequestMediaTypesAttribute : AuthorizationFilterAttribute
    {
        bool allowJson = false;
        bool allowFormUrlEncoded = false;
        
        public PreventUnsupportedRequestMediaTypesAttribute(bool allowJson = false, bool allowFormUrlEncoded = false)
        {
            this.allowJson = allowJson;
            this.allowFormUrlEncoded = allowFormUrlEncoded;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Content != null &&
                actionContext.Request.Content.Headers.ContentType != null)
            {
                if (allowJson && 
                    actionContext.Request.Content.Headers.ContentType.MediaType == JsonMediaTypeFormatter.DefaultMediaType.MediaType)
                {
                    return;
                }
                
                if (allowFormUrlEncoded && 
                    actionContext.Request.Content.Headers.ContentType.MediaType == FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType)
                {
                    return;
                }

                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, new { ErrorMessage = Messages.UnsupportedMediaType });
            }
        }
    }
}
