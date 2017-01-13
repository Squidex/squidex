using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Write
{
    public abstract class SchemaCommand : AppCommand, ISchemaCommand
    {
        public Guid SchemaId { get; set; }
    }
}
