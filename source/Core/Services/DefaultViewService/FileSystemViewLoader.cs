﻿/*
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

namespace Thinktecture.IdentityServer.Core.Services.DefaultViewService
{
    /// <summary>
    /// View loader that loads HTML templates from the file system.
    /// </summary>
    public class FileSystemViewLoader : IViewLoader
    {
        readonly string _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemViewLoader"/> class.
        /// </summary>
        /// <param name="directory">The directory from which to load HTML templates.</param>
        /// <exception cref="System.ArgumentNullException">directory</exception>
        public FileSystemViewLoader(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            
            _directory = directory;
        }

        /// <summary>
        /// Loads the specified page.
        /// If the file "page.html" exists, then that will be used for the entire template.
        /// If the file "_layout.html" exists, then that will be used for the layout template.
        /// If the file "_page.html" exists, then that will be used for the inner template.
        /// If only one of "_layout.html" or "_page.html" exists, then the embedded assets template is used for the template missing from the file system.
        /// If none of the above files exist, then <c>null</c> is returned.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public string Load(string page)
        {
            if (Directory.Exists(_directory))
            {
                var name = page + ".html";
                var path = Path.Combine(_directory, name);

                // look for full file with name login.html
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }

                var layoutName = Path.Combine(_directory, "_layout.html");
                string layout = null;
                if (File.Exists(layoutName))
                {
                    layout = File.ReadAllText(layoutName);
                }

                // look for partial with name _login.html
                name = "_" + name;
                path = Path.Combine(_directory, name);
                if (File.Exists(path))
                {
                    var partial = File.ReadAllText(path);

                    if (layout != null)
                    {
                        return AssetManager.ApplyContentToLayout(layout, partial);
                    }

                    return AssetManager.LoadLayoutWithContent(partial);
                }

                // no partial, but layout might exist
                if (layout != null)
                {
                    // so load embedded asset page, but use custom layout
                    var content = AssetManager.LoadPage(page);
                    return AssetManager.ApplyContentToLayout(layout, content);
                }
            }

            return null;
        }
    }
}
