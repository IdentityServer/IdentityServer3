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
using System.IO;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class FileSystemWithEmbeddedFallbackViewLoader : IViewLoader
    {
        readonly FileSystemViewLoader file;
        readonly EmbeddedAssetsViewLoader embedded;

        public FileSystemWithEmbeddedFallbackViewLoader()
            : this(GetDefaultDirectory())
        {
        }

        public FileSystemWithEmbeddedFallbackViewLoader(string directory)
        {
            this.file = new FileSystemViewLoader(directory);
            this.embedded = new EmbeddedAssetsViewLoader();
        }

        static string GetDefaultDirectory()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "assets");
            return path;
        }
        
        public string Load(string name)
        {
            var value = file.Load(name);
            if (value == null)
            {
                value = embedded.Load(name);
            }
            return value;
        }
    }
}
