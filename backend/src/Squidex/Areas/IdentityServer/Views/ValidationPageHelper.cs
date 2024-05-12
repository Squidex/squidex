// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Squidex.Areas.IdentityServer.Views;

[HtmlTargetElement("div", Attributes = "error-for")]
public class ValidationPageHelper : TagHelper
{
    private readonly IHtmlHelper htmlHelper;

    [HtmlAttributeName("error-for")]
    public ModelExpression For { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public ValidationPageHelper(IHtmlHelper htmlHelper)
    {
        this.htmlHelper = htmlHelper;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.Clear();

        if (htmlHelper is IViewContextAware viewContextAware)
        {
            viewContextAware.Contextualize(ViewContext);
        }

        if (ViewContext.ModelState[For.Name]?.ValidationState != ModelValidationState.Invalid)
        {
            return;
        }

        var message = htmlHelper.ValidationMessage(For.Name);

        if (message == null)
        {
            return;
        }

        output.Content.AppendHtml("<span class=\"errors\">");
        output.Content.AppendHtml(message);
        output.Content.AppendHtml("</span>");
        output.Attributes.Add("class", "errors-container");
    }
}
