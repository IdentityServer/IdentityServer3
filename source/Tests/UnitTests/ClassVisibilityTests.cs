using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Thinktecture.IdentityServer.Core;
using Xunit;

namespace Thinktecture.IdentityServer.Tests
{
    public class InternalizedDependencyCompatibilityTests
    {
        private readonly List<Assembly> _bannedAssemblies;

        public InternalizedDependencyCompatibilityTests()
        {
            _bannedAssemblies = new List<Assembly>();
            var excludedAssemblies = new[]
            {
                "Owin"
            };
            foreach (var referencedAssembly in typeof(Constants).Assembly.GetReferencedAssemblies())
            {
                var asm = Assembly.Load(referencedAssembly);
                if (!asm.GlobalAssemblyCache && !excludedAssemblies.Any(x => string.Equals(referencedAssembly.Name, x, StringComparison.OrdinalIgnoreCase)))
                {
                    _bannedAssemblies.Add(asm);
                }
            }
            
        }
        [Fact]
        public void NoTypesShouldExposeAnyIlMergedAssemblies()
        {
            var assembly = typeof(Constants).Assembly;
            var errors = new List<string>();
            foreach (var type in assembly.GetExportedTypes())
            {
                errors.AddRange(CheckConstructor(type));
                //API controllers need to be public if we don't want to rewrite autofac/webapi controller discovery
                if (type.BaseType != typeof (ApiController))
                {
                    errors.AddRange(CheckProperties(type));
                    errors.AddRange(CheckMethods(type));
                }
            }
            Console.WriteLine(errors.Count);
            Assert.Equal(new string[]{}, errors);
        }

        private IEnumerable<string> CheckMethods(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                foreach (var parameterInfo in method.GetParameters())
                {
                    if(_bannedAssemblies.Any(x => x == parameterInfo.ParameterType.Assembly))
                        yield return string.Format("ILMERGED TYPE EXPOSED(method) {3} {0}.{1}({2})",
                        type.FullName,
                        method.Name,
                        FormatParameters(method.GetParameters()), method.ReturnType.Name);
                }
                if(_bannedAssemblies.Any(x => x == method.ReturnType.Assembly))
                    yield return
                        string.Format("ILMERGED TYPE EXPOSED(method) {3} {0}.{1}({2})",
                        type.FullName,
                        method.Name,
                        FormatParameters(method.GetParameters()), method.ReturnType.Name);
            }
        }

        private IEnumerable<string> CheckProperties(Type type)
        {
            foreach (var propInfo in type.GetProperties())
            {
                if(_bannedAssemblies.Any(x => x == propInfo.PropertyType.Assembly))
                    yield return string.Format("ILMERGED TYPE EXPOSED(property): {2} : {0}.{1}", type.FullName, propInfo.Name, propInfo.PropertyType.FullName);
            }
        }

        private IEnumerable<string> CheckConstructor(Type type)
        {
            foreach (var ctor in type.GetConstructors())
            {
                foreach (var parameterInfo in ctor.GetParameters())
                {
                    if(_bannedAssemblies.Any(x => x == parameterInfo.ParameterType.Assembly))
                        yield return string.Format("ILMERGED TYPE EXPOSED {0}.ctor({1})", type.FullName,
                        FormatParameters(ctor.GetParameters()));
                }
            }
        }

        private static string FormatParameters(ParameterInfo[] parameters)
        {
            return string.Join(",", parameters.Select(x => x.ParameterType.Name));
        }
    }
}
