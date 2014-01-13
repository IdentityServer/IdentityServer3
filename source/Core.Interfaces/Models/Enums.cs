namespace Thinktecture.IdentityServer.Core.Models
{
    public enum Flows
    {
        Code,
        Implicit,
        Hybrid
    }

    public enum SubjectTypes
    {
        Global,
        PPID
    };

    public enum ApplicationTypes
    {
        Web,
        Native
    };

    public enum SigningKeyTypes
    {
        Default,
        ClientSecret
    };
}
