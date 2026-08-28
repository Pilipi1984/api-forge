using ApiForge.ApplicationCore.Interfaces;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ApiForge.Infrastructure.YamlToJson
{
    public class ConvertYamlToJson : IConvertYamlToJson
    {
        public async Task<string> ConvertAsync(string yamlPath)
        {
            if (string.IsNullOrWhiteSpace(yamlPath))
                throw new ArgumentNullException(nameof(yamlPath));

            if (!File.Exists(yamlPath))
                throw new FileNotFoundException("YAML file not found.", yamlPath);

            var yaml = await File.ReadAllTextAsync(yamlPath).ConfigureAwait(false);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var obj = deserializer.Deserialize<object>(yaml);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(obj, options);
            return json;
        }
    }
}
