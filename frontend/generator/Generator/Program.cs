using NJsonSchema;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.TypeScript;
using System.Text.RegularExpressions;

namespace Generator;

internal partial class Program
{
    static async Task Main(string[] args)
    {
        var cacheFile = "cache.json";

        if (!File.Exists(cacheFile) || !args.Contains("--cache"))
        {
            var httpClient = new HttpClient();
            var schemaResponse = await httpClient.GetAsync("https://localhost:5001/api/swagger/v1/swagger.json");
            var schemaText = await schemaResponse.Content.ReadAsStringAsync();

            File.WriteAllText(cacheFile, schemaText);
        }

        var (rootPath, codePath) = GetCodePath();
        Console.WriteLine($"Using root folder {rootPath}");

        var document = await OpenApiDocument.FromJsonAsync(File.ReadAllText(cacheFile));

        foreach (var (typeName, schema) in document.Components.Schemas.ToList())
        {
            if (typeName.Equals("AssetDto"))
            {
                if (schema.ActualProperties.TryGetValue("tags", out var tags))
                {
                    tags.IsNullableRaw = false;
                    tags.IsRequired = true;
                }
            }

            if (typeName.Equals("LanguageDto"))
            {
                schema.Properties.Remove("nativeName");
            }

            if (typeName.Equals("AppDto") || typeName.Equals("TeamDto"))
            {
                if (schema.ActualProperties.ContainsKey("created") && !schema.ActualProperties.ContainsKey("createdBy"))
                {
                    schema.Properties["createdBy"] = new JsonSchemaProperty
                    {
                        Description = "The user that has created the app.",
                        Type = JsonObjectType.String,
                        IsNullableRaw = true,
                        IsRequired = false,
                        IsReadOnly = true,
                    };
                }

                if (schema.ActualProperties.ContainsKey("lastModified") && !schema.ActualProperties.ContainsKey("lastModifiedBy"))
                {
                    schema.Properties["lastModifiedBy"] = new JsonSchemaProperty
                    {
                        Description = "The user that has updated the app.",
                        Type = JsonObjectType.String,
                        IsNullableRaw = true,
                        IsRequired = false,
                        IsReadOnly = true,
                    };
                }
            }

            foreach (var (name, property) in schema.ActualProperties)
            {
                property.IsReadOnly = true;

                if (property.Type == JsonObjectType.String && !property.IsRequired)
                {
                    property.IsNullableRaw = true;
                }

                if (name.Equals("referenceFields"))
                {
                    property.IsNullableRaw = false;
                    property.IsRequired = true;
                }

                if (name.Equals("schemaName"))
                {
                    property.IsNullableRaw = false;
                    property.IsRequired = true;
                }

                if (name.Equals("schemaDisplayName"))
                {
                    property.IsNullableRaw = false;
                    property.IsRequired = true;
                }
            }

            if (document.Components.Schemas.TryGetValue("ErrorDto", out var error))
            {
                document.Components.Schemas.Remove("ErrorDto");
                document.Components.Schemas["ServerErrorDto"] = error;
            }

            if (!typeName.EndsWith("Dto") && schema.Type == JsonObjectType.Object)
            {
                document.Components.Schemas.Remove(typeName);
                document.Components.Schemas[$"{typeName}Dto"] = schema;
            }
        }

        var extensionFile = Path.Combine(rootPath, @"src\\app\\shared\\model\\custom.ts");
        var extensionCode = File.ReadAllText(extensionFile);

        var classes =
            ClassNameRegex().Matches(extensionCode)
                .Select(m => m.Groups["ClassName"].Value)
                .ToArray();

        var settings = new TypeScriptClientGeneratorSettings
        {
            GenerateClientClasses = false,
            GenerateClientInterfaces = false,
        };
        settings.TypeScriptGeneratorSettings.EnumStyle = TypeScriptEnumStyle.StringLiteral;
        settings.TypeScriptGeneratorSettings.ExportTypes = true;
        settings.TypeScriptGeneratorSettings.ExtendedClasses = classes;
        settings.TypeScriptGeneratorSettings.ExtensionCode = extensionCode;
        settings.TypeScriptGeneratorSettings.GenerateConstructorInterface = true;
        settings.TypeScriptGeneratorSettings.InlineNamedDictionaries = true;
        settings.TypeScriptGeneratorSettings.TemplateDirectory = Path.Combine(codePath, "Templates");

        var generator = new TypeScriptClientGenerator(document, settings);

        var code = generator.GenerateFile();

        code = code.Replace("I{ [key: string]", "{ [key: string]");
        code = code.Replace(": Date,", ": DateTime,");
        code = code.Replace(": Date;", ": DateTime;");
        code = code.Replace(": Date |", ": DateTime |");
        code = code.Replace("DtoDto", "Dto");

        var targetFolder = Path.Combine(rootPath, @"src\\app\\shared\\model\\generated.ts");

        ValidateComputed(code);

        File.WriteAllText(targetFolder, code);
        Console.WriteLine("Code Generation completed");
    }

    private static (string RootPath, string CodePath) GetCodePath()
    {
        var folder = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (folder != null)
        {
            var subFolders = folder.GetDirectories();
            if (subFolders.Any(x => x.Name == "src"))
            {
                return (folder.FullName, Path.Combine(folder.FullName, "generator/Generator"));
            }

            folder = folder.Parent;
        }

        throw new InvalidOperationException("Cannot find code folder.");
    }

    private static void ValidateComputed(string code)
    {
        var lines = code.Split('\n');

        var i = 0;
        foreach (var line in lines)
        {
            ValidateLine(lines, line, i);
            i++;
        }
    }

    private static void ValidateLine(string[] lines, string line, int index)
    {
        var computed = ComputedRegex().Match(line);
        if (!computed.Success)
        {
            return;
        }

        var cacheKey = computed.Groups["Name"].Value;

        var previousLine = lines[index - 1];
        var property = PropertyRegex().Match(previousLine);
        if (!property.Success)
        {
            Console.WriteLine($"Line {index}: Cannot find property for computed '{cacheKey}'");
            return;
        }

        var propertyName = property.Groups["Name"].Value;
        if (propertyName != cacheKey)
        {
            Console.WriteLine($"Line {index}: Invalid cache key '{cacheKey}' for property '{propertyName}'");
        }
    }

    [GeneratedRegex("return this\\.compute\\('(?<Name>[^']*)',")]
    private static partial Regex ComputedRegex();

    [GeneratedRegex("class (?<ClassName>[^\\)]*) extends generated\\.")]
    private static partial Regex ClassNameRegex();

    [GeneratedRegex("public get (?<Name>[^(]*)\\(\\) {")]
    private static partial Regex PropertyRegex();
}