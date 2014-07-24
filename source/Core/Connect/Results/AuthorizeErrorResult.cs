/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    // TODO : brock cleanup
    //public class AuthorizeErrorResult : IHttpActionResult
    //{
    //    private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

    //    private readonly HttpRequestMessage _request;
    //    private readonly AuthorizeError _error;

    //    public AuthorizeErrorResult(HttpRequestMessage request, AuthorizeError error)
    //    {
    //        _request = request;
    //        _error = error;
    //    }

    //    public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        return await Execute();
    //    }

    //    private async Task<HttpResponseMessage> Execute()
    //    {
    //        var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);

    //        //if (_error.ErrorType == ErrorTypes.User)
    //        {
    //            //var env = _request.GetOwinEnvironment();
    //            //var errorModel = new Thinktecture.IdentityServer.Core.Models.ViewService.ErrorModel
    //            //{
    //            //    SiteName = _op
    //            //    ErrorMessage = _error.Error
    //            //};
    //            //var errorResult = new Thinktecture.IdentityServer.Core.Authentication.ErrorActionResult();
    //            //return await errorResult.GetResponseMessage();
    //        }
            
    //        if (_error.ErrorType == ErrorTypes.Client)
    //        {
    //            string character;
    //            if (_error.ResponseMode == Constants.ResponseModes.Query ||
    //                _error.ResponseMode == Constants.ResponseModes.FormPost)
    //            {
    //                character = "?";
    //            }
    //            else
    //            {
    //                character = "#";
    //            }

    //            var url = string.Format("{0}{1}error={2}", _error.ErrorUri.AbsoluteUri, character, _error.Error);

    //            if (_error.State.IsPresent())
    //            {
    //                url = string.Format("{0}&state={1}", url, _error.State);
    //            }

    //            responseMessage.Headers.Location = new Uri(url);
    //            Logger.Info("Redirecting to: " + url);

    //            return responseMessage;
    //        }

    //        throw new ArgumentException("errorType");
    //    }
    //}
}