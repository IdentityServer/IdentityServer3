/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Views
{
    class ErrorActionResult : HtmlStreamActionResult
    {
        public ErrorActionResult(IViewService viewSvc, IDictionary<string, object> env, ErrorViewModel model)
            : base(async () => await viewSvc.Error(env, model))
        {
            if (viewSvc == null) throw new ArgumentNullException("viewSvc");
            if (env == null) throw new ArgumentNullException("env");
            if (model == null) throw new ArgumentNullException("model");
        }
    }
}
