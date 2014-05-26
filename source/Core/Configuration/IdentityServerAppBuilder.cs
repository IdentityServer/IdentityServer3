///*
// * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
// * see license
// */
//using Owin;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Thinktecture.IdentityServer.Core.Configuration
//{
//    public class IdentityServerAppBuilder : IAppBuilder
//    {
//        IAppBuilder inner;
//        public IdentityServerAppBuilder(IAppBuilder inner)
//        {
//            this.inner = inner;
//        }

//        public object Build(Type returnType)
//        {
//            return inner.Build(returnType); ;
//        }

//        public IAppBuilder New()
//        {
//            return inner.New();
//        }

//        public IDictionary<string, object> Properties
//        {
//            get { return inner.Properties; }
//        }

//        public IAppBuilder Use(object middleware, params object[] args)
//        {
//            return inner.Use(middleware, args);
//        }
//    }
//}
