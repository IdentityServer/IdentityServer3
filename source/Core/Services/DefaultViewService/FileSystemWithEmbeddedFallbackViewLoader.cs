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

using System;
using System.IO;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// View loader implementation that uses a combination of the file system view loader 
    /// and the embedded assets view loader. This allows for some templates to be defined 
    /// via the file system, while using the embedded assets templates for all others.
    /// </summary>
    public class FileSystemWithEmbeddedFallbackViewLoader : IViewLoader
    {
        readonly FileSystemViewLoader file;
        readonly EmbeddedAssetsViewLoader embedded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWithEmbeddedFallbackViewLoader"/> class.
        /// </summary>
        public FileSystemWithEmbeddedFallbackViewLoader()
            : this(GetDefaultDirectory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWithEmbeddedFallbackViewLoader"/> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public FileSystemWithEmbeddedFallbackViewLoader(string directory)
        {
            this.file = new FileSystemViewLoader(directory);
            this.embedded = new EmbeddedAssetsViewLoader();
        }

        static string GetDefaultDirectory()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "templates");
            return path;
        }

        /// <summary>
        /// Loads the HTML for the named view.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public async Task<string> LoadAsync(string name)
        {
            var value = await file.LoadAsync(name);
            if (value == null)
            {
                value = await embedded.LoadAsync(name);
            }
            return value;
        }
    }
}
