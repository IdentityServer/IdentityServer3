/*
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

using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    internal class AggregatePermissionsStore : IPermissionsStore
    {
        readonly IPermissionsStore[] stores;

        public AggregatePermissionsStore(params IPermissionsStore[] stores)
        {
            if (stores == null) throw new ArgumentNullException("stores");

            this.stores = stores;
        }
        
        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var result = 
                await stores
                    .Select(x => x.LoadAllAsync(subject))
                    .Aggregate(async (t1, t2) => (await t1).Union(await t2));

            var query = 
                from item in result
                group item by item.ClientId into grp
                let scopes = (from g in grp select g.Scopes).Aggregate((s1, s2)=>s1.Union(s2).Distinct())
                select new Consent
                {
                    ClientId = grp.Key,
                    Subject = subject,
                    Scopes = scopes
                };

            return query;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            foreach (var store in stores)
            {
                await store.RevokeAsync(subject, client);
            }
        }
    }
}
