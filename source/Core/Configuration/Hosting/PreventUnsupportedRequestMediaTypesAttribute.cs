/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Resources;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal class PreventUnsupportedRequestMediaTypesAttribute : AuthorizationFilterAttribute
    {
        readonly bool allowJson = false;
        readonly bool allowFormUrlEncoded = false;
        
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
