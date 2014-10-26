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
using System.ComponentModel;
using System.IO;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    class AssetManager
    {
        public const string PageAssetsNamespace = "Thinktecture.IdentityServer.Core.Services.DefaultViewService.Assets";
        public const string FontAssetsNamespace = PageAssetsNamespace + ".libs.bootstrap.fonts";

        const string PagesPrefix = PageAssetsNamespace + ".app.";
        const string Layout = PagesPrefix + "layout.html";
        const string FormPostResponse = PagesPrefix + "FormPostResponse.html";

        static readonly ResourceCache cache = new ResourceCache();

        const string PageNameTemplate = PagesPrefix + "{0}" + ".html";
        public static string LoadPage(string pageName)
        {
            pageName = String.Format(PageNameTemplate, pageName);
            return LoadResourceString(pageName);
        }

        public static string ApplyContentToLayout(string layout, string content)
        {
            return Format(layout, new { pageContent = content });
        }
        
        public static string LoadLayoutWithContent(string content)
        {
            var layout = LoadResourceString(AssetManager.Layout);
            return ApplyContentToLayout(layout, content);
        }

        public static string LoadLayoutWithPage(string pageName)
        {
            var pageContent = LoadPage(pageName);
            return LoadLayoutWithContent(pageContent);
        }
        
        public static string LoadFormPost(string rootUrl, string redirect_uri, string fields)
        {
            return LoadResourceString(AssetManager.FormPostResponse, 
                new{
                    rootUrl,
                    redirect_uri,
                    fields
                }
            );
        }
        
        static string LoadResourceString(string name)
        {
            string value = cache.Read(name);
            if (value == null)
            {
                var assembly = typeof(AssetManager).Assembly;
                using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    value = sr.ReadToEnd();
                    cache.Write(name, value);
                }
            }
            return value;
        }

        static string LoadResourceString(string name, object data)
        {
            string value = LoadResourceString(name);
            value = Format(value, data);
            return value;
        }

        static string Format(string value, IDictionary<string, object> data)
        {
            foreach (var key in data.Keys)
            {
                var val = data[key];
                val = val ?? String.Empty;
                value = value.Replace("{" + key + "}", val.ToString());
            }
            return value;
        }

        public static string Format(string value, object data)
        {
            return Format(value, Map(data));
        }
        
        static IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            
            if (dictionary == null) 
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
    }
}

