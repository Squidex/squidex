// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Extensions.Actions.Algolia;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Discourse;

[FlowStep(
    Title = "Discourse",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M16.137 0C7.376 0 0 7.037 0 15.721V32l16.134-.016C24.895 31.984 32 24.676 32 15.995S24.888 0 16.137 0zm.336 6.062a9.862 9.862 0 0 1 5.119 1.555l-.038-.023a.747.747 0 0 1 .05.033l-.033-.021c.288.183.529.353.762.534l-.022-.016c.058.044.094.073.131.103l-.018-.014c.218.174.411.34.597.514l-.005-.005a9.48 9.48 0 0 1 .639.655l.009.01c.073.082.154.176.233.272l.014.018c.053.06.116.133.177.206l.013.017-.052-.047-.008-.007c.104.126.218.273.328.423l.02.028.001.001-.001-.001c-.01-.018.005.005.019.028l.024.042c.145.206.301.451.445.704l.025.048c.131.226.273.51.402.801l.025.063a9.504 9.504 0 0 1 .802 3.853c0 5.38-4.401 9.741-9.831 9.741a9.866 9.866 0 0 1-4.106-.888l.061.025-6.39 1.43 1.78-5.672a7.888 7.888 0 0 1-.293-.584l-.025-.061a8.226 8.226 0 0 1-.254-.617l-.022-.068A1.043 1.043 0 0 1 7 19.017l-.022-.067a8.428 8.428 0 0 1-.246-.829l-.014-.067a9.402 9.402 0 0 1-.265-2.248c0-5.381 4.403-9.744 9.834-9.744l.194.002h-.01z'/></svg>",
    IconColor = "#eB6121",
    Display = "Post to discourse",
    Description = "Create a post or topic at discourse.",
    ReadMore = "https://www.discourse.org/")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed record DiscourseFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
{
    [AbsoluteUrl]
    [LocalizedRequired]
    [Display(Name = "Server Url", Description = "The url to the discourse server.")]
    [Editor(FlowStepEditor.Url)]
    public Uri Url { get; set; }

    [LocalizedRequired]
    [Display(Name = "Api Key", Description = "The api key to authenticate to your discourse server.")]
    [Editor(FlowStepEditor.Text)]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    [Display(Name = "Api User", Description = "The api username to authenticate to your discourse server.")]
    [Editor(FlowStepEditor.Text)]
    public string ApiUsername { get; set; }

    [LocalizedRequired]
    [Display(Name = "Text", Description = "The text as markdown.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string Text { get; set; }

    [Display(Name = "Title", Description = "The optional title when creating new topics.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? Title { get; set; }

    [Display(Name = "Topic", Description = "The optional topic id.")]
    [Editor(FlowStepEditor.Text)]
    public int? Topic { get; set; }

    [Display(Name = "Category", Description = "The optional category id.")]
    [Editor(FlowStepEditor.Text)]
    public int? Category { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var url = $"{Url.ToString().TrimEnd('/')}/posts.json?api_key={ApiKey}&api_username={ApiUsername}";

        var body = new Dictionary<string, object?>
        {
            ["title"] = Title,
        };

        if (Topic != null)
        {
            body.Add("topic_id", Topic.Value);
        }

        if (Category != null)
        {
            body.Add("category", Category.Value);
        }

        body["raw"] = Text;

        var jsonRequest = executionContext.SerializeJson(body);

        var httpClient =
            executionContext.Resolve<IHttpClientFactory>()
                .CreateClient("DiscourseAction");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json"),
        };

        request.Headers.TryAddWithoutValidation("Api-Key", ApiKey);
        request.Headers.TryAddWithoutValidation("Api-Username", ApiUsername);

        var (_, dump) = await httpClient.SendAsync(executionContext, request, jsonRequest, ct);

        if (Topic != null)
        {
            executionContext.Log("Created post in topic {Topic}", dump);
        }
        else
        {
            executionContext.Log("Created Topic", dump);
        }

        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new AlgoliaAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
