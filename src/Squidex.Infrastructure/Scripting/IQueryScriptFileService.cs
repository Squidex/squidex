using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Infrastructure.Scripting
{
    public interface IQueryScriptFileService
    {
        string GetScriptContents(string appName, string schemaName, string scriptName);
    }
}
