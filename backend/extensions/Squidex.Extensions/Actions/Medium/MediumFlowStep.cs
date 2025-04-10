// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Squidex.Flows;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Medium;

[FlowStep(
    Title = "Medium",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M3.795 8.48a1.239 1.239 0 0 0-.404-1.045l-2.987-3.6v-.537H9.68l7.171 15.727 6.304-15.727H32v.537l-2.556 2.449a.749.749 0 0 0-.284.717v18a.749.749 0 0 0 .284.716l2.493 2.449v.537H19.39v-.537l2.583-2.509c.253-.253.253-.328.253-.717V10.392l-7.187 18.251h-.969L5.703 10.392v12.232a1.69 1.69 0 0 0 .463 1.404l3.36 4.08v.536H-.001v-.537l3.36-4.08c.36-.371.52-.893.435-1.403V8.48z'/></svg>",
    IconColor = "#00ab6c",
    Display = "Post to Medium",
    Description = "Create a new story or post at medium.",
    ReadMore = "https://medium.com/")]
internal sealed record MediumFlowStep : FlowStep
{
    [LocalizedRequired]
    [Display(Name = "Access Token", Description = "The self issued access token.")]
    [Editor(FlowStepEditor.Text)]
    public string AccessToken { get; set; }

    [LocalizedRequired]
    [Display(Name = "Title", Description = "The title, used for the url.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string Title { get; set; }

    [LocalizedRequired]
    [Display(Name = "Content", Description = "The content, either html or markdown.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression]
    public string Content { get; set; }

    [Display(Name = "Canonical Url", Description = "The original home of this content, if it was originally published elsewhere.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? CanonicalUrl { get; set; }

    [Display(Name = "Tags", Description = "The optional comma separated list of tags.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? Tags { get; set; }

    [Display(Name = "Publication Id", Description = "Optional publication id.")]
    [Editor(FlowStepEditor.Text)]
    public string? PublicationId { get; set; }

    [Display(Name = "Is Html", Description = "Indicates whether the content is markdown or html.")]
    [Editor(FlowStepEditor.Text)]
    public bool IsHtml { get; set; }

    public override async ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var jsonSerializer = executionContext.Resolve<IJsonSerializer>();

        var httpClient =
            executionContext.Resolve<IHttpClientFactory>()
                .CreateClient("MediumAction");

        string path;
        if (!string.IsNullOrWhiteSpace(PublicationId))
        {
            path = $"/v1/publications/{PublicationId}/posts";
        }
        else
        {
            var meRequest = BuildGetRequest("/v1/me");

            var (responseString, _) = await httpClient.SendAsync(executionContext, meRequest, null, ct);
            var responseJson = jsonSerializer.Deserialize<UserResponse>(responseString);

            var id = responseJson.Data?.Id;

            path = $"/v1/users/{id}/posts";
        }

        var requestBody = new
        {
            title = Title,
            contentFormat = IsHtml ? "html" : "markdown",
            content = Content,
            canonicalUrl = CanonicalUrl,
            tags = ParseTags(jsonSerializer),
        };

        var requestJson = jsonSerializer.Serialize(requestBody);

        var (_, dump) = await httpClient.SendAsync(executionContext, BuildPostRequest(path, requestJson), requestJson, ct);

        executionContext.Log("Post created", dump);
        return Next();
    }

    private HttpRequestMessage BuildPostRequest(string path, string body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

        request.Headers.Add("Authorization", $"Bearer {AccessToken}");

        return request;
    }

    private HttpRequestMessage BuildGetRequest(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        request.Headers.Add("Authorization", $"Bearer {AccessToken}");

        return request;
    }

    private string[]? ParseTags(IJsonSerializer jsonSerializer)
    {
        if (string.IsNullOrWhiteSpace(Tags))
        {
            return null;
        }

        try
        {
            return jsonSerializer.Deserialize<string[]>(Tags);
        }
        catch
        {
            return Tags.Split(',');
        }
    }

    private sealed class UserResponse
    {
        public UserResponseData Data { get; set; }
    }

    private sealed class UserResponseData
    {
        public string Id { get; set; }
    }
}
