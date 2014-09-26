/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public class ClaimComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim x, Claim y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;

            return (x.Type == y.Type &&
                    x.Value == y.Value);
        }

        public int GetHashCode(Claim claim)
        {
            if (Object.ReferenceEquals(claim, null)) return 0;

            int typeHash = claim.Type == null ? 0 : claim.Type.GetHashCode();
            int valueHash = claim.Value == null ? 0 : claim.Value.GetHashCode();

            return typeHash ^ valueHash;
        }
    }
}
