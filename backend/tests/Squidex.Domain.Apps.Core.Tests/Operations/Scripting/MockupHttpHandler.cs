// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

internal sealed class MockupHttpHandler(HttpResponseMessage response) : HttpMessageHandler
{
    private HttpRequestMessage currentRequest;
    private string? currentContent;
    private string? currentContentType;

    public void ShouldBeMethod(HttpMethod method)
    {
        Assert.Equal(method, currentRequest.Method);
    }

    public void ShouldBeUrl(string url)
    {
        Assert.Equal(url, currentRequest.RequestUri?.ToString());
    }

    public void ShouldBeHeader(string key, string value)
    {
        Assert.Equal(value, currentRequest.Headers.GetValues(key).FirstOrDefault());
    }

    public void ShouldBeBody(string content, string contentType)
    {
        Assert.Equal(content, currentContent);
        Assert.Equal(contentType, currentContentType);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

        currentRequest = request;

        if (request.Content is HttpContent body)
        {
            currentContent = await body.ReadAsStringAsync(cancellationToken);
            currentContentType = body.Headers.ContentType?.MediaType;
        }

        // The caller disposes the response, therefore every request gets its own copy of the template.
        var result = new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(await response.Content.ReadAsStringAsync(cancellationToken)),
        };

        foreach (var (key, values) in response.Content.Headers)
        {
            result.Content.Headers.Remove(key);
            result.Content.Headers.TryAddWithoutValidation(key, values);
        }

        foreach (var (key, values) in response.Headers)
        {
            result.Headers.TryAddWithoutValidation(key, values);
        }

        return result;
    }
}
