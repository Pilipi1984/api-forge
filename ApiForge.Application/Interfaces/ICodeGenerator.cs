using ApiForge.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiForge.Application.Interfaces
{
    public interface ICodeGenerator
    {
        Task<GeneratedSolution> GenerateAsync(ApiDefinition definition);
    }
}
