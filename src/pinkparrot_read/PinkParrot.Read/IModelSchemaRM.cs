using System;

namespace PinkParrot.Read.Models
{
    public interface IModelSchemaRM
    {
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
        Guid SchemaId { get; set; }
    }
}