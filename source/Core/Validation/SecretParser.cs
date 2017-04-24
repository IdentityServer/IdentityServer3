using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class SecretParser
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IEnumerable<ISecretParser> _parsers;

        public SecretParser(IEnumerable<ISecretParser> parsers)
        {
            _parsers = parsers;
        }

        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            // see if a registered parser finds a secret on the request
            ParsedSecret bestSecret = null;
            foreach (var parser in _parsers)
            {
                var parsedSecret = await parser.ParseAsync(environment);
                if (parsedSecret != null)
                {
                    Logger.DebugFormat("Parser found secret: {0}", parser.GetType().Name);

                    bestSecret = parsedSecret;

                    if (parsedSecret.Type != Constants.ParsedSecretTypes.NoSecret)
                    {
                        break;
                    }
                }
            }

            if (bestSecret != null)
            {
                Logger.InfoFormat("Secret id found: {0}", bestSecret.Id);
                return bestSecret;
            }

            Logger.InfoFormat("Parser found no secret");
            return null;
        }
    }
}