using System.Collections.Generic;

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// Options class to configure discovery endpoint
    /// </summary>
    public class DiscoveryOptions
    {
        /// <summary>
        /// Show endpoints
        /// </summary>
        public bool ShowEndpoints { get; set; }

        /// <summary>
        /// Show signing keys
        /// </summary>
        public bool ShowKeySet { get; set; }

        /// <summary>
        /// Show identity scopes
        /// </summary>
        public bool ShowIdentityScopes { get; set; }

        /// <summary>
        /// Show resource scopes
        /// </summary>
        public bool ShowResourceScopes { get; set; }

        /// <summary>
        /// Show identity claims
        /// </summary>
        public bool ShowClaims { get; set; }

        /// <summary>
        /// Show response types
        /// </summary>
        public bool ShowResponseTypes { get; set; }

        /// <summary>
        /// Show response modes
        /// </summary>
        public bool ShowResponseModes { get; set; }

        /// <summary>
        /// Show standard grant types
        /// </summary>
        public bool ShowGrantTypes { get; set; }

        /// <summary>
        /// Show custom grant types
        /// </summary>
        public bool ShowCustomGrantTypes { get; set; }

        /// <summary>
        /// Show token endpoint authentication methods
        /// </summary>
        public bool ShowTokenEndpointAuthenticationMethods { get; set; }

        /// <summary>
        /// Adds custom entries to the discovery document
        /// </summary>
        public Dictionary<string, object> CustomEntries { get; set; }

        /// <summary>
        /// Initializes with default values
        /// </summary>
        public DiscoveryOptions()
        {
            ShowEndpoints = true;
            ShowKeySet = true;
            ShowIdentityScopes = true;
            ShowResourceScopes = true;
            ShowClaims = true;
            ShowResponseTypes = true;
            ShowResponseModes = true;
            ShowGrantTypes = true;
            ShowCustomGrantTypes = true;
            ShowTokenEndpointAuthenticationMethods = true;
            CustomEntries = new Dictionary<string, object>();
        }
    }
}