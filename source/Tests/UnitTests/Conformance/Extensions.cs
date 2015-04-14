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

using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ViewModels;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace IdentityServer3.Tests.Conformance
{
    public static class Extensions
    {
        public static NameValueCollection RequestAuthorizationCode(this IdentityServerHost host, string client_id, string redirect_uri, string scope, string nonce = null)
        {
            var state = Guid.NewGuid().ToString();

            var url = host.GetAuthorizeUrl(client_id, redirect_uri, scope, "code", state, nonce);
            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseQueryString();

            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);

            return query;
        }
        
        public static void Login(this IdentityServerHost host, string username = "bob")
        {
            var resp = host.GetLoginPage();
            var model = resp.GetPageModel<LoginViewModel>();

            var user = host.Users.Single(x=>x.Username == username);
            resp = host.PostForm(model.LoginUrl, new LoginCredentials { Username = user.Username, Password = user.Password }, model.AntiForgery);
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        public static HttpResponseMessage GetLoginPage(this IdentityServerHost host, SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage() { ReturnUrl = host.Url.EnsureTrailingSlash() };
            var signInId = host.WriteMessageToCookie(msg);
            return host.Get(host.GetLoginUrl(signInId));
        }

        public static string WriteMessageToCookie<T>(this IdentityServerHost host, T msg)
            where T : Message
        {
            var request_headers = new Dictionary<string, string[]>();
            var response_headers = new Dictionary<string, string[]>();
            var env = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "https"},
                {"owin.RequestHeaders", request_headers},
                {"owin.ResponseHeaders", response_headers},
                {Constants.OwinEnvironment.IdentityServerBasePath, "/"},
            };

            var ctx = new OwinContext(env);
            var signInCookie = new MessageCookie<T>(ctx, host.Options);
            var id = signInCookie.Write(msg);

            CookieHeaderValue cookie;
            if (!CookieHeaderValue.TryParse(response_headers["Set-Cookie"].First(), out cookie))
            {
                throw new InvalidOperationException("MessageCookie failed to issue cookie");
            }

            host.Client.AddCookies(cookie.Cookies);

            return id;
        }

        public static HttpResponseMessage Get(this IdentityServerHost host, string path)
        {
            if (!path.StartsWith("http")) path = host.Url.EnsureTrailingSlash() + path;
            
            var result = host.Client.GetAsync(path).Result;
            host.ProcessCookies(result);

            return result;
        }

        public static T Get<T>(this IdentityServerHost host, string path)
        {
            var result = host.Get(path);
            result.IsSuccessStatusCode.Should().BeTrue();
            return result.Content.ReadAsAsync<T>().Result;
        }

        static NameValueCollection Map(object values, AntiForgeryTokenViewModel xsrf = null)
        {
            var coll = values as NameValueCollection;
            if (coll != null) return coll;

            coll = new NameValueCollection();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
            {
                var val = descriptor.GetValue(values);
                if (val == null) val = "";
                coll.Add(descriptor.Name, val.ToString());
            }
            
            if (xsrf != null)
            {
                coll.Add(xsrf.Name, xsrf.Value);
            }
            
            return coll;
        }

        static string ToFormBody(NameValueCollection coll)
        {
            var sb = new StringBuilder();
            foreach (var item in coll.AllKeys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.AppendFormat("{0}={1}", item, coll[item].ToString());
            }
            return sb.ToString();
        }

        public static HttpResponseMessage PostForm(this IdentityServerHost host, string path, object value, AntiForgeryTokenViewModel xsrf = null)
        {
            if (!path.StartsWith("http")) path = host.Url.EnsureTrailingSlash() + path;
            
            var form = Map(value, xsrf);
            var body = ToFormBody(form);
            var content = new StringContent(body, Encoding.UTF8, FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType);
            
            var response = host.Client.PostAsync(path, content).Result;
            host.ProcessCookies(response);

            return response;
        }

        public static HttpResponseMessage PostJson<T>(this IdentityServerHost host, string path, T value)
        {
            return host.Client.PostAsJsonAsync(path, value).Result;
        }

        static void ProcessCookies(this IdentityServerHost host, HttpResponseMessage response)
        {
            var cookies = response.GetCookies();
            foreach(var cookie in cookies)
            {
                if (cookie.Expires != null && cookie.Expires < host.UtcNow)
                {
                    var names = cookie.Cookies.Select(x=>x.Name);
                    host.Client.RemoveCookies(names);
                }
                else
                {
                    host.Client.AddCookies(cookie.Cookies);
                }
            }
        }

        public static string GetLoginUrl(this IdentityServerHost host, string signInId)
        {
            return host.Url.EnsureTrailingSlash() + Constants.RoutePaths.Login + "?signin=" + signInId;
        }
        public static string GetAuthorizeUrl(this IdentityServerHost host, string client_id = null, string redirect_uri = null, string scope = null, string response_type = null, string state = null, string nonce = null)
        {
            var disco = host.GetDiscoveryDocument();
            disco["authorization_endpoint"].Should().NotBeNull();
            disco["response_types_supported"].Should().NotBeNull();
            var arr = (JArray)disco["response_types_supported"];
            var values = arr.Select(x=>x.ToString());
            values.Should().Contain("code");

            var url = disco["authorization_endpoint"].ToString();
            
            var query = "";
            if (response_type.IsPresent())
            {
                query += "&response_type=" + HttpUtility.UrlEncode(response_type);
            }
            if (scope.IsPresent())
            {
                query += "&scope=" + HttpUtility.UrlEncode(scope);
            }
            if (client_id.IsPresent())
            {
                query += "&client_id=" + HttpUtility.UrlEncode(client_id);
            }
            if (redirect_uri.IsPresent())
            {
                query += "&redirect_uri=" + HttpUtility.UrlEncode(redirect_uri);
            }
            if (state.IsPresent())
            {
                query += "&state=" + HttpUtility.UrlEncode(state);
            }
            if (nonce.IsPresent())
            {
                query += "&nonce=" + HttpUtility.UrlEncode(nonce);
            }

            if (query.StartsWith("&"))
            {
                url += "?" + query.Substring(1);
            }
            
            return url;
        }
        public static string GetTokenUrl(this IdentityServerHost host)
        {
            return host.Url.EnsureTrailingSlash() + Constants.RoutePaths.Oidc.Token;
        }
        
        public static string GetUserInfoUrl(this IdentityServerHost host)
        {
            return host.Url.EnsureTrailingSlash() + Constants.RoutePaths.Oidc.UserInfo;
        }

        public static string GetDiscoveryUrl(this IdentityServerHost host)
        {
            return host.Url.EnsureTrailingSlash() + Constants.RoutePaths.Oidc.DiscoveryConfiguration;
        }

        public static JObject GetDiscoveryDocument(this IdentityServerHost host)
        {
            var disco_url = host.GetDiscoveryUrl();
            var result = host.Client.GetAsync(disco_url).Result;
            
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var json = result.Content.ReadAsStringAsync().Result;
            var data = JObject.Parse(json);

            string[] https_checks = new string[]{
                "issuer", "jwks_uri", "authorization_endpoint", "token_endpoint", "userinfo_endpoint", "end_session_endpoint", "check_session_iframe"
            };
            foreach (var url in https_checks)
            {
                data[url].Should().NotBeNull();
                data[url].ToString().Should().StartWith("https://");
            }

            var issuer = host.Url;
            if (issuer.EndsWith("/")) issuer = issuer.Substring(0, issuer.Length - 1);
            data["issuer"].ToString().Should().Be(issuer);

            var jwks = data["jwks_uri"].ToString();
            result = host.Client.GetAsync(jwks).Result;
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            return data;
        }

        public static X509Certificate2 GetSigningCertificate(this IdentityServerHost host)
        {
            var meta = host.GetDiscoveryDocument();

            meta["jwks_uri"].Should().NotBeNull();
            var jwks = meta["jwks_uri"].ToString();
            
            var result = host.Client.GetAsync(jwks).Result;
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var json = result.Content.ReadAsStringAsync().Result;
            var data = JObject.Parse(json);
            data["keys"].Should().NotBeNull();

            var keys = (JArray)data["keys"];
            var rsa = keys.FirstOrDefault(x => (string)x["kty"] == "RSA" && (string)x["use"] == "sig");
            rsa.Should().NotBeNull();

            var certs = (JArray)rsa["x5c"];
            certs.Should().NotBeNull();

            var cert = (string)certs.First();
            cert.Should().NotBeNull();

            var bytes = Convert.FromBase64String(cert);
            var ret = new X509Certificate2(bytes);
            ret.Should().NotBeNull();
            
            return ret;
        }

        public static T GetPageModel<T>(string html)
        {
            var match = "<script id='modelJson' type='application/json'>";
            var start = html.IndexOf(match);
            var end = html.IndexOf("</script>", start);
            var content = html.Substring(start + match.Length, end - start - match.Length);
            var json = HttpUtility.HtmlDecode(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T GetPageModel<T>(this HttpResponseMessage resp)
        {
            var html = resp.Content.ReadAsStringAsync().Result;
            return GetPageModel<T>(html);
        }

        public static void AssertPage(this HttpResponseMessage resp, string name)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            resp.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            var html = resp.Content.ReadAsStringAsync().Result;

            var match = Regex.Match(html, "<div class='container page-(.*)' ng-cloak>");
            match.Groups[1].Value.Should().Be(name);
        }

        public static void AssertCookie(this HttpResponseMessage resp, string name)
        {
            var cookies = resp.GetCookies();
            var cookie = cookies.SingleOrDefault(x => x.Cookies.Any(y=>y.Name == name));
            cookie.Should().NotBeNull();
        }

        public static void AddCookies(this HttpClient client, IEnumerable<string> cookies)
        {
            foreach (var c in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", c);
            }
        }

        public static void AddCookies(this HttpClient client, IEnumerable<CookieState> cookies)
        {
            foreach (var c in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", c.ToString());
            }
        }

        public static void RemoveCookies(this HttpClient client, IEnumerable<string> names)
        {
            foreach(var name in names)
            {
                client.RemoveCookie(name);
            }
        }

        public static void RemoveCookie(this HttpClient client, string name)
        {
            var cookies = client.DefaultRequestHeaders.Where(x => x.Key == "Cookie").ToArray();
            client.DefaultRequestHeaders.Remove("Cookie");

            var cookieValues = cookies.SelectMany(x=>x.Value).Where(x=>!x.StartsWith(name+"="));
            client.AddCookies(cookieValues);
        }

        public static IEnumerable<CookieHeaderValue> GetCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                var cookies = new List<CookieHeaderValue>();
                foreach (var value in values)
                {
                    CookieHeaderValue cookie;
                    if (CookieHeaderValue.TryParse(value, out cookie))
                    {
                        cookies.Add(cookie);
                    }
                }
                return cookies;
            }
            return Enumerable.Empty<CookieHeaderValue>();
        }

        public static NameValueCollection ParseHashFragment(this Uri uri)
        {
            var url = uri.AbsoluteUri;
            if (!url.Contains("#")) return new NameValueCollection();

            var hash = url.Substring(url.IndexOf('#') + 1);
            return new Uri("http://foo?" + hash).ParseQueryString();
        }
        
        public static JObject ReadJsonObject(this HttpResponseMessage resp)
        {
            var json = resp.Content.ReadAsStringAsync().Result;
            return JObject.Parse(json);
        }
    }
}
