﻿/*
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

using System.Web.Http;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Results;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class ResultExtensions
    {
        public static IHttpActionResult TokenResponse(this ApiController controller, TokenResponse response)
        {
            return new TokenResult(response);
        }

        public static IHttpActionResult TokenErrorResponse(this ApiController controller, string error)
        {
            return new TokenErrorResult(error);
        }
    }
}