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

namespace Thinktecture.IdentityServer.Core.Views
{
    public class FileSystemViewLoader : IViewLoader
    {
        string directory;

        public FileSystemViewLoader(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            if (!Directory.Exists(directory)) throw new InvalidOperationException(String.Format("{0} is an invalid directory", directory));
            
            this.directory = directory;
        }

        public string Load(string name)
        {
            var path = Path.Combine(directory, name);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return null;
        }
    }
}
