using Microsoft.AspNetCore.Mvc;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class GraphQLQueryDto
{
    /// <summary>
    /// The GraphQL query.
    /// </summary>
    [FromQuery(Name = "query")]
    public string Query { get; set; }

    /// <summary>
    /// The optional operation name.
    /// </summary>
    [FromQuery(Name = "operationName")]
    public string? OperationName { get; set; }

    /// <summary>
    /// The optional variables.
    /// </summary>
    [FromQuery(Name = "variables")]
    public string? Variables { get; set; }
}
