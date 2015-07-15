/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Models;
using System;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    internal class ExternalClaimsFilterUserService : IUserService
    {
        readonly IExternalClaimsFilter filter;
        readonly IUserService inner;

        public ExternalClaimsFilterUserService(IExternalClaimsFilter filter, IUserService inner)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (inner == null) throw new ArgumentNullException("inner");

            this.filter = filter;
            this.inner = inner;
        }

        public Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return inner.PreAuthenticateAsync(context);
        }

        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            return inner.AuthenticateLocalAsync(context);
        }

        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            context.ExternalIdentity.Claims = filter.Filter(context.ExternalIdentity.Provider, context.ExternalIdentity.Claims);
            return inner.AuthenticateExternalAsync(context);
        }

        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return inner.PostAuthenticateAsync(context);
        }
        
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return inner.GetProfileDataAsync(context);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return inner.IsActiveAsync(context);
        }

        public Task SignOutAsync(SignOutContext context)
        {
            return inner.SignOutAsync(context);
        }
    }
}