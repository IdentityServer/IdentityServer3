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

using Autofac;
using System;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services.Default;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal static class AutofacConfig
    {
        const string DecoratorRegistrationName = "decorator.inner";

        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public static IContainer Configure(IdentityServerOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.Factory == null) throw new InvalidOperationException("null factory");

            IdentityServerServiceFactory fact = options.Factory;
            fact.Validate();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule(options));

            return builder.Build();
        }

        internal static void RegisterDefaultType<T, TDefault>(this ContainerBuilder builder, Registration<T> registration, string name = null)
            where T : class
            where TDefault : T
        {
            if (registration != null)
            {
                builder.Register(registration, name);
            }
            else
            {
                if (name == null)
                {
                    builder.RegisterType<TDefault>().As<T>();
                }
                else
                {
                    builder.RegisterType<TDefault>().Named<T>(name);
                }
            }
        }

        internal static void RegisterDefaultInstance<T, TDefault>(this ContainerBuilder builder, Registration<T> registration, string name = null)
            where T : class
            where TDefault : class, T, new()
        {
            if (registration != null)
            {
                builder.Register(registration, name);
            }
            else
            {
                if (name == null)
                {
                    builder.RegisterInstance(new TDefault()).As<T>();
                }
                else
                {
                    builder.RegisterInstance(new TDefault()).Named<T>(name);
                }
            }
        }

        internal static void RegisterDecorator<T, TDecorator>(this ContainerBuilder builder, string name)
            where T : class
            where TDecorator : T
        {
            builder.RegisterType<TDecorator>();
            builder.Register<T>(ctx =>
            {
                var inner = Autofac.Core.ResolvedParameter.ForNamed<T>(name);
                return ctx.Resolve<TDecorator>(inner);
            });
        }

        internal static void RegisterDecoratorDefaultInstance<T, TDecorator, TDefault>(this ContainerBuilder builder, Registration<T> registration)
            where T : class
            where TDecorator : T
            where TDefault : class, T, new()
        {
            builder.RegisterDefaultInstance<T, TDefault>(registration, DecoratorRegistrationName);
            builder.RegisterDecorator<T, TDecorator>(DecoratorRegistrationName);
        }

        internal static void RegisterDecoratorDefaultType<T, TDecorator, TDefault>(this ContainerBuilder builder, Registration<T> registration)
            where T : class
            where TDecorator : T
            where TDefault : class, T, new()
        {
            builder.RegisterDefaultType<T, TDefault>(registration, DecoratorRegistrationName);
            builder.RegisterDecorator<T, TDecorator>(DecoratorRegistrationName);
        }

        internal static void RegisterDecorator<T, TDecorator>(this ContainerBuilder builder, Registration<T> registration)
            where T : class
            where TDecorator : T
        {
            builder.Register(registration, DecoratorRegistrationName);
            builder.RegisterType<TDecorator>();
            builder.Register<T>(ctx =>
            {
                var inner = Autofac.Core.ResolvedParameter.ForNamed<T>(DecoratorRegistrationName);
                return ctx.Resolve<TDecorator>(inner);
            });
        }

        internal static void Register(this ContainerBuilder builder, Registration registration, string name = null)
        {
            if (registration.Instance != null)
            {
                var reg = builder.Register(ctx=>registration.Instance).SingleInstance();
                if (name != null)
                {
                    reg.Named(name, registration.DependencyType);
                }
                else
                {
                    reg.As(registration.DependencyType);
                }
                switch (registration.Mode)
                {
                    case RegistrationMode.Singleton:
                        // this is the only option when Instance is provided
                        break;
                    case RegistrationMode.InstancePerHttpRequest:
                        throw new InvalidOperationException("RegistrationMode.InstancePerHttpRequest can't be used when an Instance is provided.");
                    case RegistrationMode.InstancePerUse:
                        throw new InvalidOperationException("RegistrationMode.InstancePerUse can't be used when an Instance is provided.");
                }
            }
            else if (registration.Type != null)
            {
                var reg = builder.RegisterType(registration.Type);
                if (name != null)
                {
                    reg.Named(name, registration.DependencyType);
                }
                else
                {
                    reg.As(registration.DependencyType);
                }

                switch(registration.Mode)
                {
                    case RegistrationMode.InstancePerHttpRequest:
                        reg.InstancePerRequest(); break;
                    case RegistrationMode.Singleton:
                        reg.SingleInstance(); break;
                    case RegistrationMode.InstancePerUse:
                        // this is the default behavior
                        break;
                }
            }
            else if (registration.Factory != null)
            {
                var reg = builder.Register(ctx => registration.Factory(new AutofacDependencyResolver(ctx.Resolve<IComponentContext>())));
                if (name != null)
                {
                    reg.Named(name, registration.DependencyType);
                }
                else
                {
                    reg.As(registration.DependencyType);
                }

                switch (registration.Mode)
                {
                    case RegistrationMode.InstancePerHttpRequest:
                        reg.InstancePerRequest(); break;
                    case RegistrationMode.InstancePerUse:
                        // this is the default behavior
                        break;
                    case RegistrationMode.Singleton:
                        throw new InvalidOperationException("RegistrationMode.Singleton can't be used when using a factory function.");
                }
            }
            else
            {
                var message = "No type or factory found on registration " + registration.GetType().FullName; 
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }

            foreach(var item in registration.AdditionalRegistrations)
            {
                builder.Register(item, item.Name);
            }
        }
    }
}