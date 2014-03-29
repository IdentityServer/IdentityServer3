/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    static class ModelStateDictionaryExtensions
    {
        public static IEnumerable<string> GetErrors(this ModelStateDictionary modelState)
        {
            if (modelState == null) return Enumerable.Empty<string>();
            var errors =
                from item in modelState
                where item.Value.Errors.Any()
                from err in item.Value.Errors
                select err.ErrorMessage;
            return errors;
        }

        public static string GetError(this ModelStateDictionary modelState)
        {
            return modelState.GetErrors().FirstOrDefault();
        }

    }
}
