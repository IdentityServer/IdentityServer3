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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Thinktecture.IdentityServer.Core.Views
{
    class AssetManager
    {
        const string Prefix = "Thinktecture.IdentityServer.Core.Views.Embedded.Assets.app.";
        const string Layout = Prefix + "layout.html";
        const string FormPostResponse = Prefix + "FormPostResponse.html";

        static readonly ResourceCache cache = new ResourceCache();

        const string PageUrlTemplate = "assets/app.{0}.html";
        public static string LoadLayout(string pageName)
        {
            var pageUrl = String.Format(PageUrlTemplate, pageName);
            return LoadResourceString(AssetManager.Layout, new { pageUrl });
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

