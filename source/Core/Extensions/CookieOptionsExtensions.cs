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

namespace IdentityServer3.Core.Configuration
{
    internal static class CookieOptionsExtensions
    {
        public static string GetCookieName(this CookieOptions options, string name)
        {
            return options.Prefix + name;
        }

        const string SessionCookieName = "idsvr.session";

        public static string GetSessionCookieName(this CookieOptions options)
        {
            return options.GetCookieName(SessionCookieName);
        }

        internal static bool? CalculateRememberMeFromUserInput(this CookieOptions options, bool? userInput)
        {
            // the browser will only send 'true' if the user has checked the checkbox
            // it will pass nothing if the user does not check the checkbox
            // this check here is to establish if the user deliberatly did not check the checkbox
            // or if the checkbox was not presented as an option (and thus AllowRememberMe is not allowed)
            // true means they did check it, false means they did not, null means they were not presented with the choice
            if (options.AllowRememberMe)
            {
                if (userInput != true)
                {
                    userInput = false;
                }
            }
            else
            {
                userInput = null;
            }
            
            return userInput;
        }
    }
}