/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Thinktecture.IdentityServer.Core.Assets
{
    public class LayoutModel
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string SwitchUrl { get; set; }
        public string Page { get; set; }
        public string PageUrl 
        { 
            get
            {
                if (String.IsNullOrWhiteSpace(Page)) return null;
                return "assets/app." + Page + ".html";
            } 
        }
        public object PageModel { get; set; }
    }

    class AssetManager
    {
        public static string GetLayoutHtml(LayoutModel model, string rootUrl)
        {
            if (model == null) throw new ArgumentNullException("model");

            if (rootUrl == null) rootUrl = "";
            if (rootUrl.EndsWith("/")) rootUrl = rootUrl.Substring(0, rootUrl.Length - 1);

            model.Server = model.Server ?? "Thinktecture IdentityServer";
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
            return LoadResourceString("Thinktecture.IdentityServer.Core.Assets.app.layout.html",
                new {
                    server = model.Server,
                    rootUrl,
                    layoutModel = json, 
                });
        }

        static ConcurrentDictionary<string, string> ResourceStrings = new ConcurrentDictionary<string, string>();
        internal static string LoadResourceString(string name)
        {
            string value;
            if (!ResourceStrings.TryGetValue(name, out value))
            {
                var assembly = typeof(AssetManager).Assembly;
                using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    ResourceStrings[name] = value = sr.ReadToEnd();
                }
            }
            return value;
        }

        internal static string LoadResourceString(string name, IDictionary<string, object> values)
        {
            string value = LoadResourceString(name);
            foreach(var key in values.Keys)
            {
                value = value.Replace("{" + key + "}", values[key].ToString());
            }
            return value;
        }
        
        internal static string LoadResourceString(string name, object values)
        {
            return LoadResourceString(name, Map(values));
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

