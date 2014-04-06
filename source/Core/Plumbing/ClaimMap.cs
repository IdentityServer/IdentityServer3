/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public class ClaimMap
    {
        static Dictionary<string, string> ClaimTypeMap = BuildClaimTypeMap();

        static Dictionary<string, string> BuildClaimTypeMap()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("actort", "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor");
            dictionary.Add("birthdate", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth");
            dictionary.Add("email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            dictionary.Add("family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
            dictionary.Add("gender", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender");
            dictionary.Add("given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
            dictionary.Add("sub", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            dictionary.Add("nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            dictionary.Add("website", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage");
            // changed from unique_name
            dictionary.Add("name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            dictionary.Add("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier");
            dictionary.Add("scp", "http://schemas.microsoft.com/identity/claims/scope");
            dictionary.Add("tid", "http://schemas.microsoft.com/identity/claims/tenantid");
            dictionary.Add("acr", "http://schemas.microsoft.com/claims/authnclassreference");
            dictionary.Add("adfs1email", "http://schemas.xmlsoap.org/claims/EmailAddress");
            dictionary.Add("adfs1upn", "http://schemas.xmlsoap.org/claims/UPN");
            dictionary.Add("amr", "http://schemas.microsoft.com/claims/authnmethodsreferences");
            dictionary.Add("auth_time", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant");
            dictionary.Add("authmethod", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod");
            dictionary.Add("certapppolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/applicationpolicy");
            dictionary.Add("certauthoritykeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/authoritykeyidentifier");
            dictionary.Add("certbasicconstraints", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/basicconstraints");
            dictionary.Add("certeku", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/eku");
            dictionary.Add("certissuer", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuer");
            dictionary.Add("certissuername", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuername");
            dictionary.Add("certkeyusage", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/keyusage");
            dictionary.Add("certnotafter", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notafter");
            dictionary.Add("certnotbefore", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notbefore");
            dictionary.Add("certpolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatepolicy");
            dictionary.Add("certpublickey", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa");
            dictionary.Add("certrawdata", "http://schemas.microsoft.com/2012/12/certificatecontext/field/rawdata");
            dictionary.Add("certserialnumber", "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber");
            dictionary.Add("certsignaturealgorithm", "http://schemas.microsoft.com/2012/12/certificatecontext/field/signaturealgorithm");
            dictionary.Add("certsubject", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subject");
            dictionary.Add("certsubjectaltname", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/san");
            dictionary.Add("certsubjectkeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/subjectkeyidentifier");
            dictionary.Add("certsubjectname", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subjectname");
            dictionary.Add("certtemplateinformation", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplateinformation");
            dictionary.Add("certtemplatename", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplatename");
            dictionary.Add("certthumbprint", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint");
            dictionary.Add("certx509version", "http://schemas.microsoft.com/2012/12/certificatecontext/field/x509version");
            dictionary.Add("clientapplication", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-application");
            dictionary.Add("clientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip");
            dictionary.Add("clientuseragent", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-user-agent");
            dictionary.Add("commonname", "http://schemas.xmlsoap.org/claims/CommonName");
            dictionary.Add("denyonlyprimarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid");
            dictionary.Add("denyonlyprimarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid");
            dictionary.Add("denyonlysid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid");
            dictionary.Add("devicedispname", "http://schemas.microsoft.com/2012/01/devicecontext/claims/displayname");
            dictionary.Add("deviceid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier");
            dictionary.Add("deviceismanaged", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ismanaged");
            dictionary.Add("deviceostype", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ostype");
            dictionary.Add("deviceosver", "http://schemas.microsoft.com/2012/01/devicecontext/claims/osversion");
            dictionary.Add("deviceowner", "http://schemas.microsoft.com/2012/01/devicecontext/claims/userowner");
            dictionary.Add("deviceregid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/registrationid");
            dictionary.Add("endpointpath", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-endpoint-absolute-path");
            dictionary.Add("forwardedclientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-forwarded-client-ip");
            dictionary.Add("group", "http://schemas.xmlsoap.org/claims/Group");
            dictionary.Add("groupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid");
            dictionary.Add("idp", "http://schemas.microsoft.com/identity/claims/identityprovider");
            dictionary.Add("insidecorporatenetwork", "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork");
            dictionary.Add("isregistereduser", "http://schemas.microsoft.com/2012/01/devicecontext/claims/isregistereduser");
            //dictionary.Add("ppid", ClaimTypes.PPID);
            dictionary.Add("primarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid");
            dictionary.Add("primarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid");
            dictionary.Add("proxy", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-proxy");
            dictionary.Add("pwdchgurl", "http://schemas.microsoft.com/ws/2012/01/passwordchangeurl");
            dictionary.Add("pwdexpdays", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationdays");
            dictionary.Add("pwdexptime", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationtime");
            dictionary.Add("relyingpartytrustid", "http://schemas.microsoft.com/2012/01/requestcontext/claims/relyingpartytrustid");
            dictionary.Add("role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            dictionary.Add("upn", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            dictionary.Add("winaccountname", "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname");

            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
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
