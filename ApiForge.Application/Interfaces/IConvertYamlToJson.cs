using System;
using System.Collections.Generic;
using System.Text;

namespace ApiForge.ApplicationCore.Interfaces
{
    public interface IConvertYamlToJson
    {
        Task<string> ConvertAsync(string yamlPath);
    }
}
