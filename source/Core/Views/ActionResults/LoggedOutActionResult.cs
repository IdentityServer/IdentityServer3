/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Views
{
    class LoggedOutActionResult : HtmlStreamActionResult
    {
        public LoggedOutActionResult(IViewService viewSvc, IDictionary<string, object> env, LoggedOutViewModel model)
            : base(async () => await viewSvc.LoggedOut(env, model))
        {
            if (viewSvc == null) throw new ArgumentNullException("viewSvc");
            if (env == null) throw new ArgumentNullException("env");
            if (model == null) throw new ArgumentNullException("model");
        }
    }
}
