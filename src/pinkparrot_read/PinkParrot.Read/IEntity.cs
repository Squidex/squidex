using System;

namespace PinkParrot.Read.Models
{
    public interface IModelSchemaRM1
    {
        DateTime Created { get; set; }
        string Hints { get; set; }
        string Label { get; set; }
        DateTime Modified { get; set; }
        string Name { get; set; }
        Guid SchemaId { get; set; }
    }
}