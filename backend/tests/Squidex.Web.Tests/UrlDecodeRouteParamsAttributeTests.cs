// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Squidex.Web;

public class UrlDecodeRouteParamsAttributeTests
{
    [Fact]
    public void Should_url_decode_params()
    {
        var sut = new UrlDecodeRouteParamsAttribute();

        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor
        {
            FilterDescriptors = new List<FilterDescriptor>()
        });

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>
        {
            ["key"] = "path%2Fto%2Fsomething"
        }, null!);

        sut.OnActionExecuting(actionExecutingContext);

        Assert.Equal("path/to/something", actionExecutingContext.ActionArguments["key"]);
    }
}
