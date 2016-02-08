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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.ResponseHandling;
using IdentityServer3.Core.Results;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    using IdentityServer3.Core.Models;

    /// <summary>
    /// OpenID Connect end session endpoint
    /// </summary>
    [SecurityHeaders]
    [NoCache]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    internal class EndSessionController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly EndSessionRequestValidator _validator;
        private readonly EndSessionResponseGenerator _generator;
        private readonly ClientListCookie _clientListCookie;
        private readonly IClientStore _clientStore;
        private readonly SessionCookie _sessionCookie;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndSessionController"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="clientListCookie">The client list.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="sessionCookie">The session cookie.</param>
        public EndSessionController(IdentityServerOptions options, EndSessionRequestValidator validator, EndSessionResponseGenerator generator, ClientListCookie clientListCookie, IClientStore clientStore, SessionCookie sessionCookie)
        {
            _options = options;
            _validator = validator;
            _generator = generator;
            _clientListCookie = clientListCookie;
            _clientStore = clientStore;
            _sessionCookie = sessionCookie;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<IHttpActionResult> Logout()
        {
            Logger.Info("Start end session request");

            NameValueCollection parameters;

            if (Request.Method == HttpMethod.Get)
            {
                parameters = Request.RequestUri.ParseQueryString();
            }
            else if (Request.Method == HttpMethod.Post)
            {
                parameters = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            }
            else
            {
                throw new InvalidOperationException("invalid HTTP method");
            }

            var result = await _validator.ValidateAsync(parameters, User as ClaimsPrincipal);
            if (result.IsError)
            {
                // if anything went wrong, ignore the params the RP sent
                return new LogoutResult(null, Request.GetOwinEnvironment(), this._options);
            }
        
            var message = _generator.CreateSignoutMessage(_validator.ValidatedRequest);

            Logger.Info("End end session request");
            return new LogoutResult(message, Request.GetOwinEnvironment(), this._options);
        }

        /// <summary>
        /// Logout callback
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> LogoutCallback(string sid)
        {
            Logger.Info("End session callback requested");

            if (ValidateSid(sid) == false)
            {
                Logger.Error("Invalid sid passed to end session callback");
                return StatusCode(HttpStatusCode.BadRequest);
            }

            // since we verified via sid param, we can allow rendering in iframes
            Request.SetSuppressXfo();

            // get URLs for iframes
            var urls = await GetClientEndSessionUrlsAsync();
            if (urls.Any())
            {
                var msg = urls.Aggregate((x, y) => x + ", " + y);
                Logger.DebugFormat("Client end session iframe URLs: {0}", msg);
            }
            else
            {
                Logger.Debug("No client end session iframe URLs");
            }

            // relax CSP to allow those iframe origins
            ConfigureCspResponseHeader(urls);

            // clear cookies
            ClearCookies();

            // get html (with iframes)
            string html = GetEndSessionHtml(urls);
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });
        }

        private bool ValidateSid(string sid)
        {
            return IdentityModel.TimeConstantComparer.IsEqual(_sessionCookie.GetSessionId(), sid);
        }

        private void ConfigureCspResponseHeader(IEnumerable<string> urls)
        {
            // relax CSP for these origins so iframes can make requests
            var origins = urls.Select(x => x.GetOrigin());
            Request.SetAllowedCspFrameOrigins(origins);
        }

        private string GetEndSessionHtml(IEnumerable<string> urls)
        {
            // get html with iframes for SLO
            return AssetManager.LoadSignoutFrame(urls);
        }

        private async Task<IEnumerable<string>> GetClientEndSessionUrlsAsync()
        {
            // read client list to get URLs for client logout endpoints
            var clientIds = _clientListCookie.GetClients();

            // Fetch the Clients for each clientid
            var clients = new List<Client>();
            foreach (var clientId in clientIds)
            {
                var client = await _clientStore.FindClientByIdAsync(clientId);

                if (client != null)
                {
                    clients.Add(client);
                }
            }

            // get user's session id. session id will possibly 
            // be needed below to pass to client's endpoint
            var sid = _sessionCookie.GetSessionId();

            var urls = new List<string>();
            foreach (var client in clients)
            {
                if (client.LogoutUri.IsPresent())
                {
                    var url = client.LogoutUri;

                    // add session id if required
                    if (client.LogoutSessionRequired)
                    {
                        url = url.AddQueryString(Constants.ClaimTypes.SessionId + "=" + sid);
                    }

                    urls.Add(url);
                }
            }

            return urls;
        }

        private void ClearCookies()
        {
            // session id cookie
            _sessionCookie.ClearSessionId();

            // client list cookie
            _clientListCookie.Clear();
        }
    }
}