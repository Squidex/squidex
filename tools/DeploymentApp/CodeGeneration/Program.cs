// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Net.Http;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace CodeGeneration
{
    public static class Program
    {
        public static void Main()
        {
            string json = null;

            var clientHandler = new HttpClientHandler();

            clientHandler.ServerCertificateCustomValidationCallback += (message, certificate, chain, errors) => true;

            using (var client = new HttpClient(clientHandler))
            {
                json = client.GetStringAsync("http://localhost:5000/api/swagger/v1/swagger.json").Result;
            }
            
            var document = OpenApiDocument.FromJsonAsync(json).Result;

            var generatorSettings = new CSharpClientGeneratorSettings();
            generatorSettings.CSharpGeneratorSettings.Namespace = "Cosmos.ClientLibrary.Management";
            generatorSettings.GenerateClientInterfaces = true;
            generatorSettings.ExceptionClass = "CosmosManagementException";
            generatorSettings.OperationNameGenerator = new TagNameGenerator();
            generatorSettings.UseBaseUrl = false;

            var codeGenerator = new CSharpClientGenerator(document, generatorSettings);

            var code = codeGenerator.GenerateFile();

            code = code.Replace(" = new FieldPropertiesDto();", string.Empty);
            code = code.Replace(" = new RuleTriggerDto();", string.Empty);

            File.WriteAllText(@"..\DeploymentApp\Management\Generated.cs", code);
        }

        public sealed class TagNameGenerator : MultipleClientsFromOperationIdOperationNameGenerator
        {
            public override string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
            {
                if (operation.Tags?.Count == 1)
                {
                    return operation.Tags[0];
                }

                return base.GetClientName(document, path, httpMethod, operation);
            }
        }
    }
}
