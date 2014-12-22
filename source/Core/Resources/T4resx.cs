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

namespace Thinktecture.IdentityServer.Core.Resources
{
	public class EventIds
	{
			public const string LocalLoginSuccess = "LocalLoginSuccess";
	}
	public class MessageIds
	{
			public const string ClientIdRequired = "ClientIdRequired";
			public const string Invalid_scope = "Invalid_scope";
			public const string InvalidUsernameOrPassword = "InvalidUsernameOrPassword";
			public const string MissingClientId = "MissingClientId";
			public const string MissingToken = "MissingToken";
			public const string MustSelectAtLeastOnePermission = "MustSelectAtLeastOnePermission";
			public const string NoExternalProvider = "NoExternalProvider";
			public const string NoMatchingExternalAccount = "NoMatchingExternalAccount";
			public const string NoSignInCookie = "NoSignInCookie";
			public const string NoSubjectFromExternalProvider = "NoSubjectFromExternalProvider";
			public const string PasswordRequired = "PasswordRequired";
			public const string SslRequired = "SslRequired";
			public const string Unauthorized_client = "Unauthorized_client";
			public const string UnexpectedError = "UnexpectedError";
			public const string Unsupported_response_type = "Unsupported_response_type";
			public const string UnsupportedMediaType = "UnsupportedMediaType";
			public const string UsernameRequired = "UsernameRequired";
	}
	public class ScopeIds
	{
			public const string AddressDisplayName = "AddressDisplayName";
			public const string AllClaimsDisplayName = "AllClaimsDisplayName";
			public const string EmailDisplayName = "EmailDisplayName";
			public const string OfflineAccessDisplayName = "OfflineAccessDisplayName";
			public const string OpenIdDisplayName = "OpenIdDisplayName";
			public const string PhoneDisplayName = "PhoneDisplayName";
			public const string ProfileDescription = "ProfileDescription";
			public const string ProfileDisplayName = "ProfileDisplayName";
			public const string RolesDisplayName = "RolesDisplayName";
	}
}
