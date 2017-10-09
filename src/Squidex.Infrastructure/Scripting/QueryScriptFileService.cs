using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Squidex.Infrastructure.Scripting
{
    public class QueryScriptFileService : IQueryScriptFileService
    {
        private readonly string scriptsRoot;

        public QueryScriptFileService(string scriptsRoot)
        {
            this.scriptsRoot = scriptsRoot;
        }

        public string GetScriptContents(string appName, string schemaName, string scriptName)
        {
            var filePath = $"{scriptsRoot}/{appName}/{schemaName}/{scriptName}.js";

            if (!File.Exists($"{scriptsRoot}/{appName}/{schemaName}/{scriptName}.js"))
            {
                return string.Empty;
            }

            return File.ReadAllText(filePath);
        }
    }
}
