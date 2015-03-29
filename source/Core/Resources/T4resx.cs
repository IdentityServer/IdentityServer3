
#pragma warning disable 1591

namespace IdentityServer3.Core.Resources
{
	public class EventIds
	{
			public const string ClientPermissionsRevoked = "ClientPermissionsRevoked";
			public const string CspReport = "CspReport";
			public const string ExternalLoginError = "ExternalLoginError";
			public const string ExternalLoginFailure = "ExternalLoginFailure";
			public const string ExternalLoginSuccess = "ExternalLoginSuccess";
			public const string LocalLoginFailure = "LocalLoginFailure";
			public const string LocalLoginSuccess = "LocalLoginSuccess";
			public const string LogoutEvent = "LogoutEvent";
			public const string PartialLogin = "PartialLogin";
			public const string PartialLoginComplete = "PartialLoginComplete";
			public const string PreLoginFailure = "PreLoginFailure";
			public const string PreLoginSuccess = "PreLoginSuccess";
			public const string ResourceOwnerFlowLoginFailure = "ResourceOwnerFlowLoginFailure";
			public const string ResourceOwnerFlowLoginSuccess = "ResourceOwnerFlowLoginSuccess";
	}
	public class MessageIds
	{
			public const string ClientIdRequired = "ClientIdRequired";
			public const string ExternalProviderError = "ExternalProviderError";
			public const string Invalid_request = "invalid_request";
			public const string Invalid_scope = "invalid_scope";
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
			public const string Unauthorized_client = "unauthorized_client";
			public const string UnexpectedError = "UnexpectedError";
			public const string Unsupported_response_type = "unsupported_response_type";
			public const string UnsupportedMediaType = "UnsupportedMediaType";
			public const string UsernameRequired = "UsernameRequired";
	}
	public class ScopeIds
	{
			public const string Address_DisplayName = "address_DisplayName";
			public const string All_claims_DisplayName = "all_claims_DisplayName";
			public const string Email_DisplayName = "email_DisplayName";
			public const string Offline_access_DisplayName = "offline_access_DisplayName";
			public const string Openid_DisplayName = "openid_DisplayName";
			public const string Phone_DisplayName = "phone_DisplayName";
			public const string Profile_Description = "profile_Description";
			public const string Profile_DisplayName = "profile_DisplayName";
			public const string Roles_DisplayName = "roles_DisplayName";
	}
}
