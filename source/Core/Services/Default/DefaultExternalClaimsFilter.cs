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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class ExternalClaimsFilterUserService : IUserService
    {
        IExternalClaimsFilter filter;
        IUserService inner;

        public ExternalClaimsFilterUserService(IExternalClaimsFilter filter, IUserService inner)
        {
            this.filter = filter;
            this.inner = inner;
        }

        public System.Threading.Tasks.Task<Authentication.AuthenticateResult> PreAuthenticateAsync(IDictionary<string, object> env, Authentication.SignInMessage message)
        {
            return inner.PreAuthenticateAsync(env, message);
        }

        public System.Threading.Tasks.Task<Authentication.AuthenticateResult> AuthenticateLocalAsync(string username, string password, Authentication.SignInMessage message = null)
        {
            return inner.AuthenticateLocalAsync(username, password, message);
        }

        public System.Threading.Tasks.Task<Authentication.AuthenticateResult> AuthenticateExternalAsync(Models.ExternalIdentity externalUser)
        {
            externalUser.Claims = filter.Filter(externalUser.Provider, externalUser.Claims);
            return inner.AuthenticateExternalAsync(externalUser);
        }

        public System.Threading.Tasks.Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            return inner.GetProfileDataAsync(subject, requestedClaimTypes);
        }

        public System.Threading.Tasks.Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            return inner.IsActiveAsync(subject);
        }
    }

    public class AggregateExternalClaimsFilter : IExternalClaimsFilter
    {
        IExternalClaimsFilter[] filters;
        public AggregateExternalClaimsFilter(params IExternalClaimsFilter[] filters)
        {
            this.filters = filters;
        }

        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            foreach (var filter in this.filters)
            {
                claims = filter.Filter(provider, claims);
            }
            return claims;
        }
    }

    public class NormalizingClaimsFilter : IExternalClaimsFilter
    {
        IExternalClaimsFilter inner;

        public NormalizingClaimsFilter(IExternalClaimsFilter inner)
        {
            this.inner = inner;
        }

        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            claims = ClaimMap.Map(claims);

            return inner.Filter(provider, claims);
        }
    }

    public abstract class ClaimsFilterBase : IExternalClaimsFilter
    {
        readonly string provider;

        public ClaimsFilterBase(string provider)
        {
            this.provider = provider;
        }

        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            if (this.provider == provider)
            {
                claims = TransformClaims(claims);
            }

            return claims;
        }

        protected abstract IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims);
    }

    public class FacebookClaimsFilter : ClaimsFilterBase
    {
        public FacebookClaimsFilter()
            : this("Facebook")
        {
        }

        public FacebookClaimsFilter(string provider)
            : base(provider)
        {
        }

        protected override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            var nameClaim = claims.FirstOrDefault(x => x.Type == "urn:facebook:name");
            if (nameClaim != null)
            {
                var list = claims.ToList();
                list.Remove(nameClaim);
                list.RemoveAll(x => x.Type == Constants.ClaimTypes.Name);
                list.Add(new Claim(Constants.ClaimTypes.Name, nameClaim.Value));
                return list;
            }
            return claims;
        }
    }

    public class TwitterClaimsFilter : ClaimsFilterBase
    {
        public TwitterClaimsFilter()
            : this("Twitter")
        {
        }

        public TwitterClaimsFilter(string provider)
            : base(provider)
        {
        }

        protected override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            return claims.Where(x => x.Type != "urn:twitter:userid");
        }
    }
}
