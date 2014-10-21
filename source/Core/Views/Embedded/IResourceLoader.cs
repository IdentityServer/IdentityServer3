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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Views.Embedded
{
    public interface IResourceLoader
    {
        string Load(string name);
    }

    public class FileSystemWithEmbeddedFallbackLoader : IResourceLoader
    {
        FileSystemLoader file;
        EmbeddedResourceLoader embedded;

        public FileSystemWithEmbeddedFallbackLoader(string directory)
        {
            this.file = new FileSystemLoader(directory);
            this.embedded = new EmbeddedResourceLoader();
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

    public class FileSystemLoader : CachingResourceLoader
    {
        string directory;

        public FileSystemLoader(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            if (!Directory.Exists(directory)) throw new InvalidOperationException(String.Format("{0} is an invalid directory", directory));
            
            this.directory = directory;
        }

        protected override string LoadResourceString(string name)
        {
            var path = Path.Combine(directory, name);
            
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return null;
        }
    }

    public class EmbeddedResourceLoader : CachingResourceLoader
    {
        const string Prefix = "Thinktecture.IdentityServer.Core.Views.Embedded.Assets.app.";

        protected override string LoadResourceString(string name)
        {
            name = Prefix + name;

            var assembly = this.GetType().Assembly;
            var stream = assembly.GetManifestResourceStream(name);
            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
            return null;
        }
    }

    public abstract class CachingResourceLoader : IResourceLoader
    {
        ResourceCache cache = new ResourceCache();

        public string Load(string name)
        {
            var value = cache.Read(name);
            if (value == null)
            {
                value = LoadResourceString(name);
                cache.Write(name, value);
            }
            return value;
        }

        abstract protected string LoadResourceString(string name);
    }

    public class ResourceCache
    {
        ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        public string Read(string name)
        {
            string value;
            if (cache.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        public void Write(string name, string value)
        {
            cache[name] = value;
        }
    }
}
