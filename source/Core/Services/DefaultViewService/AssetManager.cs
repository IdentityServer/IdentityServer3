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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Services.DefaultViewService
{
    internal class AssetManager
    {
        public const string HTTP_ASSETS_NAMESPACE = "Thinktecture.IdentityServer.Core.Services.DefaultViewService.HttpAssets";
        public const string FONT_ASSETS_NAMESPACE = HTTP_ASSETS_NAMESPACE + ".libs.bootstrap.fonts";

        public const string PAGE_ASSETS_NAMESPACE = "Thinktecture.IdentityServer.Core.Services.DefaultViewService.PageAssets";
        const string PagesPrefix = PAGE_ASSETS_NAMESPACE + ".";
        const string Layout = PagesPrefix + "layout.html";
        const string FormPostResponse = PagesPrefix + "FormPostResponse.html";
        const string CheckSession = PagesPrefix + "checksession.html";
        const string Welcome = PagesPrefix + "welcome.html";

        static readonly ResourceCache Cache = new ResourceCache();

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
            var layout = LoadResourceString(Layout);
            return ApplyContentToLayout(layout, content);
        }

        public static string LoadLayoutWithPage(string pageName)
        {
            var pageContent = LoadPage(pageName);
            return LoadLayoutWithContent(pageContent);
        }

        public static string LoadFormPost(string rootUrl, string redirectUri, string fields)
        {
            return LoadResourceString(FormPostResponse,
                new
                {
                    rootUrl,
                    redirect_uri = redirectUri,
                    fields
                }
            );
        }

        public static string LoadCheckSession(string rootUrl, string cookieName)
        {
            return LoadResourceString(CheckSession, new
            {
                rootUrl,
                cookieName
            });
        }

        internal static string LoadWelcomePage(string applicationPath, string version)
        {
            applicationPath = applicationPath.RemoveTrailingSlash();
            return LoadResourceString(Welcome, new
            {
                applicationPath,
                version
            });
        }
        
        static string LoadResourceString(string name)
        {
            var value = Cache.Read(name);
            if (value == null)
            {
                var assembly = typeof(AssetManager).Assembly;
                using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    value = sr.ReadToEnd();
                    Cache.Write(name, value);
                }
            }
            return value;
        }

        static string LoadResourceString(string name, object data)
        {
            var value = LoadResourceString(name);
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

