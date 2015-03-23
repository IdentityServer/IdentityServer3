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

﻿
#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Resources
{
	public class EventIds
	{
			public const string CLIENT_PERMISSIONS_REVOKED = "ClientPermissionsRevoked";
			public const string CSP_REPORT = "CspReport";
			public const string EXTERNAL_LOGIN_ERROR = "ExternalLoginError";
			public const string EXTERNAL_LOGIN_FAILURE = "ExternalLoginFailure";
			public const string EXTERNAL_LOGIN_SUCCESS = "ExternalLoginSuccess";
			public const string LOCAL_LOGIN_FAILURE = "LocalLoginFailure";
			public const string LOCAL_LOGIN_SUCCESS = "LocalLoginSuccess";
			public const string LOGOUT_EVENT = "LogoutEvent";
			public const string PARTIAL_LOGIN = "PartialLogin";
			public const string PARTIAL_LOGIN_COMPLETE = "PartialLoginComplete";
			public const string PRE_LOGIN_FAILURE = "PreLoginFailure";
			public const string PRE_LOGIN_SUCCESS = "PreLoginSuccess";
			public const string RESOURCE_OWNER_FLOW_LOGIN_FAILURE = "ResourceOwnerFlowLoginFailure";
			public const string RESOURCE_OWNER_FLOW_LOGIN_SUCCESS = "ResourceOwnerFlowLoginSuccess";
	}
	public class MessageIds
	{
			public const string CLIENT_ID_REQUIRED = "ClientIdRequired";
			public const string EXTERNAL_PROVIDER_ERROR = "ExternalProviderError";
			public const string INVALID_REQUEST = "invalid_request";
			public const string INVALID_SCOPE = "invalid_scope";
			public const string INVALID_USERNAME_OR_PASSWORD = "InvalidUsernameOrPassword";
			public const string MISSING_CLIENT_ID = "MissingClientId";
			public const string MISSING_TOKEN = "MissingToken";
			public const string MUST_SELECT_AT_LEAST_ONE_PERMISSION = "MustSelectAtLeastOnePermission";
			public const string NO_EXTERNAL_PROVIDER = "NoExternalProvider";
			public const string NO_MATCHING_EXTERNAL_ACCOUNT = "NoMatchingExternalAccount";
			public const string NO_SIGN_IN_COOKIE = "NoSignInCookie";
			public const string NO_SUBJECT_FROM_EXTERNAL_PROVIDER = "NoSubjectFromExternalProvider";
			public const string PASSWORD_REQUIRED = "PasswordRequired";
			public const string SSL_REQUIRED = "SslRequired";
			public const string UNAUTHORIZED_CLIENT = "unauthorized_client";
			public const string UNEXPECTED_ERROR = "UnexpectedError";
			public const string UNSUPPORTED_RESPONSE_TYPE = "unsupported_response_type";
			public const string UNSUPPORTED_MEDIA_TYPE = "UnsupportedMediaType";
			public const string USERNAME_REQUIRED = "UsernameRequired";
	}
	public class ScopeIds
	{
			public const string ADDRESS_DISPLAY_NAME = "address_DisplayName";
			public const string ALL_CLAIMS_DISPLAY_NAME = "all_claims_DisplayName";
			public const string EMAIL_DISPLAY_NAME = "email_DisplayName";
			public const string OFFLINE_ACCESS_DISPLAY_NAME = "offline_access_DisplayName";
			public const string OPENID_DISPLAY_NAME = "openid_DisplayName";
			public const string PHONE_DISPLAY_NAME = "phone_DisplayName";
			public const string PROFILE_DESCRIPTION = "profile_Description";
			public const string PROFILE_DISPLAY_NAME = "profile_DisplayName";
			public const string ROLES_DISPLAY_NAME = "roles_DisplayName";
	}
}
