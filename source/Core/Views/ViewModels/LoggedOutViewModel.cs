/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Views
{
    public class LoggedOutViewModel : CommonViewModel
    {
        public IEnumerable<string> IFrameUrls { get; set; }
    }
}
