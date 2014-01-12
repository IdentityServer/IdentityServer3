
namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidationResult
    {
        public bool IsError { get; set; }
        public string Error { get; set; }
        public ErrorTypes ErrorType { get; set; }
        
        public ValidationResult()
        {
            ErrorType = ErrorTypes.User;
        }
    }

    public enum ErrorTypes
    {
        Client,
        User
    }
}
