using Fm2ndParser.Common;
using Fm2ndParser.Kgt;
using Fm2ndParser.Parsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace Fm2ndParser
{
    internal class GenerateCommand
    {
        private string kgtFile;

        public GenerateCommand(string kgtFile)
        {
            this.kgtFile = kgtFile;
        }
        public async Task Execute()
        {
            var baseDir = Path.GetDirectoryName(kgtFile);
            
            var contractResolver = new DynamicContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var data = await File.ReadAllTextAsync(kgtFile);
            var json = JsonConvert.DeserializeObject<KGTFile>(data, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }
    }
}