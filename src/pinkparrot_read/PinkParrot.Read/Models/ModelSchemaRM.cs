using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaRM
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Guid SchemaId { get; set; }
    }
}
