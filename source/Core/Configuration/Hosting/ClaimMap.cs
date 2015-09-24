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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class ClaimMap
    {
        static readonly Dictionary<string, string> ClaimTypeMap = BuildClaimTypeMap();

        static Dictionary<string, string> BuildClaimTypeMap()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"actort", "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor"},
                {"birthdate", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth"},
                {"email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"},
                {"family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"},
                {"gender", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender"},
                {"given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"},
                {"sub", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"},
                {"nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"},
                {"website", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage"},
                {"name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"},
                {"oid", "http://schemas.microsoft.com/identity/claims/objectidentifier"},
                {"scp", "http://schemas.microsoft.com/identity/claims/scope"},
                {"tid", "http://schemas.microsoft.com/identity/claims/tenantid"},
                {"acr", "http://schemas.microsoft.com/claims/authnclassreference"},
                {"adfs1email", "http://schemas.xmlsoap.org/claims/EmailAddress"},
                {"adfs1upn", "http://schemas.xmlsoap.org/claims/UPN"},
                {"amr", "http://schemas.microsoft.com/claims/authnmethodsreferences"},
                {"auth_time", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant"},
                {"authmethod", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod"},
                {"certapppolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/applicationpolicy"},
                {
                    "certauthoritykeyidentifier",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/extension/authoritykeyidentifier"
                },
                {
                    "certbasicconstraints",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/extension/basicconstraints"
                },
                {"certeku", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/eku"},
                {"certissuer", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuer"},
                {"certissuername", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuername"},
                {"certkeyusage", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/keyusage"},
                {"certnotafter", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notafter"},
                {"certnotbefore", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notbefore"},
                {"certpolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatepolicy"},
                {"certpublickey", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa"},
                {"certrawdata", "http://schemas.microsoft.com/2012/12/certificatecontext/field/rawdata"},
                {"certserialnumber", "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber"},
                {
                    "certsignaturealgorithm",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/field/signaturealgorithm"
                },
                {"certsubject", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subject"},
                {"certsubjectaltname", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/san"},
                {
                    "certsubjectkeyidentifier",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/extension/subjectkeyidentifier"
                },
                {"certsubjectname", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subjectname"},
                {
                    "certtemplateinformation",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplateinformation"
                },
                {
                    "certtemplatename",
                    "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplatename"
                },
                {"certthumbprint", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint"},
                {"certx509version", "http://schemas.microsoft.com/2012/12/certificatecontext/field/x509version"},
                {
                    "clientapplication",
                    "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-application"
                },
                {"clientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip"},
                {"clientuseragent", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-user-agent"},
                {"commonname", "http://schemas.xmlsoap.org/claims/CommonName"},
                {
                    "denyonlyprimarygroupsid",
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid"
                },
                {"denyonlyprimarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid"},
                {"denyonlysid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid"},
                {"devicedispname", "http://schemas.microsoft.com/2012/01/devicecontext/claims/displayname"},
                {"deviceid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier"},
                {"deviceismanaged", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ismanaged"},
                {"deviceostype", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ostype"},
                {"deviceosver", "http://schemas.microsoft.com/2012/01/devicecontext/claims/osversion"},
                {"deviceowner", "http://schemas.microsoft.com/2012/01/devicecontext/claims/userowner"},
                {"deviceregid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/registrationid"},
                {
                    "endpointpath",
                    "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-endpoint-absolute-path"
                },
                {
                    "forwardedclientip",
                    "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-forwarded-client-ip"
                },
                {"group", "http://schemas.xmlsoap.org/claims/Group"},
                {"groupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid"},
                {"idp", "http://schemas.microsoft.com/identity/claims/identityprovider"},
                {"insidecorporatenetwork", "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork"},
                {"isregistereduser", "http://schemas.microsoft.com/2012/01/devicecontext/claims/isregistereduser"},
                {"primarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid"},
                {"primarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"},
                {"proxy", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-proxy"},
                {"pwdchgurl", "http://schemas.microsoft.com/ws/2012/01/passwordchangeurl"},
                {"pwdexpdays", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationdays"},
                {"pwdexptime", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationtime"},
                {
                    "relyingpartytrustid", "http://schemas.microsoft.com/2012/01/requestcontext/claims/relyingpartytrustid"
                },
                {"role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"},
                {"upn", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"},
                {"winaccountname", "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"}
            };
            // changed from unique_name
            //dictionary.Add("ppid", ClaimTypes.PPID);

            var dictionary2 = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                if (!dictionary2.ContainsKey(item.Value))
                {
                    dictionary2.Add(item.Value, item.Key);
                }
            }

            dictionary2.Add("unique_name", "name");
            return dictionary2;
        }

        static string MapClaimType(string type)
        {
            if (ClaimTypeMap.ContainsKey(type))
            {
                return ClaimTypeMap[type];
            }

            return type;
        }

        public static IEnumerable<Claim> Map(IEnumerable<Claim> incomingClaims)
        {
            if (incomingClaims == null) return null;

            var claimsToMap = incomingClaims.Where(x => ClaimTypeMap.ContainsKey(x.Type));
            var mappedClaims = claimsToMap.Select(x => new Claim(MapClaimType(x.Type), x.Value));

            var claims = new List<Claim>();
            claims.AddRange(incomingClaims.Except(claimsToMap));
            claims.AddRange(mappedClaims);

            return claims;
        }
    }
}