using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_Code_Invalid
    {
        //client null
        //client anonymous
        //unknown client
        //wrong credentials
        //no password claim

        //unknown grant_type
        //invalid grant_type for client

        //missing code
        //invalid code
        //code for a different client
        //expirect code
        //re-used code

        //missing redirect_uri
        //invalid redirect_uri
    }
}
