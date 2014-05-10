// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Web;
using System.Xml;

namespace Microsoft.IdentityModel.Protocols
{
    /// <summary>
    /// Provides access to common WsFederation message parameters.
    /// </summary>
    [type: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]   
    public class WsFederationMessage : AuthenticationProtocolMessage
    {
        /// <summary>
        /// Creates a <see cref="WsFederationMessage"/> from the contents of a query string.
        /// </summary>
        /// <param name="queryString"> query string to extract parameters.</param>
        /// <returns>An instance of <see cref="WsFederationMessage"/>.</returns>
        /// <remarks>If 'queryString' is null or whitespace, a default <see cref="WsFederationMessage"/> is returned. Parameters are parsed from <see cref="Uri.Query"/>.</remarks>
        public static WsFederationMessage FromQueryString(string queryString)
        {
            WsFederationMessage wsFederationMessage = new WsFederationMessage();
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                wsFederationMessage.SetParameters(HttpUtility.ParseQueryString(query: queryString));
            } 

            return wsFederationMessage;
        }

        /// <summary>
        /// Creates a <see cref="WsFederationMessage"/> from the contents of a <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri"> uri string to extract parameters.</param>
        /// <returns>An instance of <see cref="WsFederationMessage"/>.</returns>
        /// <remarks><see cref="WsFederationMessage"/>.IssuerAddress is NOT set/>. Parameters are parsed from <see cref="Uri.Query"/>.</remarks>
        public static WsFederationMessage FromUri(Uri uri)
        {
            if (uri != null && uri.Query.Length > 1)
            {
                return WsFederationMessage.FromQueryString(uri.Query.Substring(1));
            }

            return new WsFederationMessage();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationMessage"/> class.
        /// </summary>
        public WsFederationMessage() : this(string.Empty) {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationMessage"/> class.
        /// </summary>
        /// <param name="issuerAddress">The endpoint of the token issuer.</param>
        public WsFederationMessage(string issuerAddress) : base(issuerAddress) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationMessage"/> class.
        /// </summary>
        /// <param name="wsFederationMessage"> message to copy.</param>        
        public WsFederationMessage(WsFederationMessage wsFederationMessage)
        {
            if (wsFederationMessage == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> keyValue in wsFederationMessage.Parameters)
            {
                SetParameter(keyValue.Key, keyValue.Value);
            }

            IssuerAddress = wsFederationMessage.IssuerAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationMessage"/> class.
        /// </summary>
        /// <param name="parameters">Enumeration of key value pairs.</param>        
        public WsFederationMessage(IEnumerable<KeyValuePair<string, string[]>> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string[]> keyValue in parameters)
            {
                foreach (string strValue in keyValue.Value)
                {
                    SetParameter(keyValue.Key, strValue);
                }
            }
        }

        /// <summary>
        /// Creates a 'wsignin1.0' message using the current contents of this <see cref="WsFederationMessage"/>.
        /// </summary>
        /// <returns>The uri to use for a redirect.</returns>

        public string CreateSignInUrl()
        {
            WsFederationMessage wsFederationMessage = new WsFederationMessage(this);
            wsFederationMessage.Wa = WsFederationActions.SignIn;
            return wsFederationMessage.BuildRedirectUrl();
        }

        /// <summary>
        /// Creates a 'wsignout1.0' message using the current contents of this <see cref="WsFederationMessage"/>.
        /// </summary>
        /// <returns>The uri to use for a redirect.</returns>
        public string CreateSignOutUrl()
        {
            WsFederationMessage wsFederationMessage = new WsFederationMessage(this);
            wsFederationMessage.Wa = WsFederationActions.SignOut;
            return wsFederationMessage.BuildRedirectUrl();
        }
        
        /// <summary>
        /// Reads the 'wresult' and returns the embeded security token.
        /// </summary>
        /// <returns>the 'SecurityToken'.</returns>
        public virtual string GetToken()
        {
            if (Wresult == null)
            {
                return null;
            }

            using (StringReader sr = new StringReader(Wresult))
            {
                XmlReader xmlReader = XmlReader.Create(sr);
                xmlReader.MoveToContent();

                WSTrustResponseSerializer serializer = new WSTrust13ResponseSerializer();
                if (serializer.CanRead(xmlReader))
                {
                    RequestSecurityTokenResponse response = serializer.ReadXml(xmlReader, new WSTrustSerializationContext());
                    return response.RequestedSecurityToken.SecurityTokenXml.OuterXml;
                }

                serializer = new WSTrustFeb2005ResponseSerializer();
                if (serializer.CanRead(xmlReader))
                {
                    RequestSecurityTokenResponse response = serializer.ReadXml(xmlReader, new WSTrustSerializationContext());
                    return response.RequestedSecurityToken.SecurityTokenXml.OuterXml;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a boolean representating if the <see cref="WsFederationMessage"/> is a 'sign-in-message'.
        /// </summary>
        public bool IsSignInMessage
        {
            get
            {
                return Wa == WsFederationActions.SignIn;
            }
        }
        
        /// <summary>
        /// Gets a boolean representating if the <see cref="WsFederationMessage"/> is a 'sign-out-message'.
        /// </summary>
        public bool IsSignOutMessage
        {
            get
            {
                return Wa == WsFederationActions.SignOut;
            }
        }

        /// <summary>
        /// Gets or sets 'wa'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Wa")]
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wa 
        { 
            get { return GetParameter(WsFederationParameterNames.Wa); }
            set { SetParameter(WsFederationParameterNames.Wa, value); }
        }

        /// <summary>
        /// Gets or sets 'wattr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wattr
        {
            get { return GetParameter(WsFederationParameterNames.Wattr); }
            set { SetParameter(WsFederationParameterNames.Wattr, value); }
        }

        /// <summary>
        /// Gets or sets 'wattrptr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wattrptr
        {
            get { return GetParameter(WsFederationParameterNames.Wattrptr); }
            set { SetParameter(WsFederationParameterNames.Wattrptr, value); }
        }

        /// <summary>
        /// Gets or sets 'wauth'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wauth
        {
            get { return GetParameter(WsFederationParameterNames.Wauth); }
            set { SetParameter(WsFederationParameterNames.Wauth, value); }
        }

        /// <summary>
        /// Gets or sets 'Wct'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wct
        {
            get { return GetParameter(WsFederationParameterNames.Wct); }
            set { SetParameter(WsFederationParameterNames.Wct, value); }
        }

        /// <summary>
        /// Gets or sets 'wa'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wctx
        {
            get { return GetParameter(WsFederationParameterNames.Wctx); }
            set { SetParameter(WsFederationParameterNames.Wctx, value); }
        }

        /// <summary>
        /// Gets or sets 'Wencoding'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wencoding
        {
            get { return GetParameter(WsFederationParameterNames.Wencoding); }
            set { SetParameter(WsFederationParameterNames.Wencoding, value); }
        }

        /// <summary>
        /// Gets or sets 'wfed'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wfed
        {
            get { return GetParameter(WsFederationParameterNames.Wfed); }
            set { SetParameter(WsFederationParameterNames.Wfed, value); }
        }

        /// <summary>
        /// Gets or sets 'wfresh'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wfresh
        {
            get { return GetParameter(WsFederationParameterNames.Wfresh); }
            set { SetParameter(WsFederationParameterNames.Wfresh, value); }
        }

        /// <summary>
        /// Gets or sets 'whr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Whr
        {
            get { return GetParameter(WsFederationParameterNames.Whr); }
            set { SetParameter(WsFederationParameterNames.Whr, value); }
        }

        /// <summary>
        /// Gets or sets 'wp'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709")]
        public string Wp
        {
            get { return GetParameter(WsFederationParameterNames.Wp); }
            set { SetParameter(WsFederationParameterNames.Wp, value); }
        }

        /// <summary>
        /// Gets or sets 'wpseudo'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wpseudo
        {
            get { return GetParameter(WsFederationParameterNames.Wpseudo); }
            set { SetParameter(WsFederationParameterNames.Wpseudo, value); }
        }

        /// <summary>
        /// Gets or sets 'wpseudoptr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wpseudoptr
        {
            get { return GetParameter(WsFederationParameterNames.Wpseudoptr); }
            set { SetParameter(WsFederationParameterNames.Wpseudoptr, value); }
        }

        /// <summary>
        /// Gets or sets 'wreply'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public string Wreply
        {
            get { return GetParameter(WsFederationParameterNames.Wreply); }
            set { SetParameter(WsFederationParameterNames.Wreply, value); }
        }

        /// <summary>
        /// Gets or sets 'wreq'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wreq
        {
            get { return GetParameter(WsFederationParameterNames.Wreq); }
            set { SetParameter(WsFederationParameterNames.Wreq, value); }
        }

        /// <summary>
        /// Gets or sets 'wreqptr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wreqptr
        {
            get { return GetParameter(WsFederationParameterNames.Wreqptr); }
            set { SetParameter(WsFederationParameterNames.Wreqptr, value); }
        }

        /// <summary>
        /// Gets or sets 'wres'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wres
        {
            get { return GetParameter(WsFederationParameterNames.Wres); }
            set { SetParameter(WsFederationParameterNames.Wres, value); }
        }

        /// <summary>
        /// Gets or sets 'wresult'.
        /// </summary>
        public string Wresult
        {
            get { return GetParameter(WsFederationParameterNames.Wresult); }
            set { SetParameter(WsFederationParameterNames.Wresult, value); }
        }

        /// <summary>
        /// Gets or sets 'wresultptr'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wresultptr
        {
            get { return GetParameter(WsFederationParameterNames.Wresultptr); }
            set { SetParameter(WsFederationParameterNames.Wresultptr, value); }
        }

        /// <summary>
        /// Gets or sets 'wtrealm'.
        /// </summary>
        [property: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]        
        public string Wtrealm
        {
            get { return GetParameter(WsFederationParameterNames.Wtrealm); }
            set { SetParameter(WsFederationParameterNames.Wtrealm, value); }
        }
    }
}