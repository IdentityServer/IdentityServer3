/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidationResult
    {
        public bool IsError { get; set; }
        public string Error { get; set; }
        public ErrorTypes ErrorType { get; set; }
        
        public ValidationResult()
        {
            IsError = true;
            ErrorType = ErrorTypes.User;
        }
    }

    public enum ErrorTypes
    {
        Client,
        User
    }
}
