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

using System.Threading.Tasks;
namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// View loaded that loads HTML templates from the embedded assets.
    /// </summary>
    public class EmbeddedAssetsViewLoader : IViewLoader
    {
        /// <summary>
        /// Loads the HTML for the named view.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<string> LoadAsync(string name)
        {
            return Task.FromResult(AssetManager.LoadLayoutWithPage(name));
        }
    }
}
