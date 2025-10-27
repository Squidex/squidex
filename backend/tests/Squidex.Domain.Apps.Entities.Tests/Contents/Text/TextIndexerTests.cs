// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable CA1859 // Use concrete types when possible for improved performance

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public abstract class TextIndexerTests : GivenContext
{
    private TextIndexingProcess? process;

    protected List<DomainId> Ids1 { get; } = [DomainId.NewGuid()];
    protected List<DomainId> Ids2 { get; } = [DomainId.NewGuid()];

    public virtual bool SupportsQuerySyntax => true;

    public virtual bool SupportsGeo => false;

    public virtual TimeSpan WaitTime => TimeSpan.FromSeconds(30);

    public abstract Task<ITextIndex> CreateSutAsync();

    [Fact]
    public async Task Should_return_content_filter_for_events_filter()
    {
        var sut = await GetProcessAsync();

        Assert.Equal(StreamFilter.Prefix("content-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_search_with_fuzzy()
    {
        if (!SupportsQuerySyntax)
        {
            return;
        }

        await CreateTextAsync(Ids1[0], "iv", "Hello");

        await SearchText(expected: Ids1, text: "helo~");
    }

    [Fact]
    public async Task Should_search_by_field()
    {
        if (!SupportsQuerySyntax)
        {
            return;
        }

        await CreateTextAsync(Ids1[0], "en", "City");

        await SearchText(expected: Ids1, text: "en:city");
    }

    [Fact]
    public async Task Should_search_by_geo()
    {
        if (!SupportsGeo)
        {
            return;
        }

        var field = Guid.NewGuid().ToString();

        // Within search radius
        await CreateGeoAsync(Ids1[0], field, 51.343391192211506, 12.401476788622826);

        // Outside of search radius
        await CreateGeoAsync(Ids2[0], field, 51.30765141427311, 12.379631713912486);

        // Within search radius and correct field.
        await SearchGeo(expected: Ids1, $"{field}.iv", 51.34641682574934, 12.401965298137707);

        // Within search radius but incorrect field.
        await SearchGeo(expected: null, "other.iv", 51.48596429889613, 12.102629469505713);
    }

    [Fact]
    public async Task Should_search_by_geojson()
    {
        if (!SupportsGeo)
        {
            return;
        }

        var field = Guid.NewGuid().ToString();

        // Within search radius
        await CreateGeoJsonAsync(Ids1[0], field, 51.343391192211506, 12.401476788622826);

        // Outside of search radius
        await CreateGeoJsonAsync(Ids2[0], field, 51.30765141427311, 12.379631713912486);

        // Within search radius and correct field.
        await SearchGeo(expected: Ids1, $"{field}.iv", 51.34641682574934, 12.401965298137707);

        // Within search radius but incorrect field.
        await SearchGeo(expected: null, "other.iv", 51.48596429889613, 12.102629469505713);
    }

    [Fact]
    public async Task Should_search_by_app()
    {
        await CreateTextAsync(Ids1[0], "iv", "Hello");

        await SearchByAppText(expected: Ids1, text: "hello");
    }

    [Fact]
    public async Task Should_index_invariant_content_and_retrieve()
    {
        await CreateTextAsync(Ids1[0], "iv", "Hello");
        await CreateTextAsync(Ids2[0], "iv", "World");

        await SearchText(expected: Ids1, text: "Hello");
        await SearchText(expected: Ids2, text: "World");

        await SearchText(expected: null, text: "Hello", SearchScope.Published);
        await SearchText(expected: null, text: "World", SearchScope.Published);
    }

    [Fact]
    public async Task Should_update_draft_only()
    {
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        await UpdateTextAsync(Ids1[0], "iv", "Version2");

        await SearchText(expected: null, text: "Version1", target: SearchScope.All);
        await SearchText(expected: null, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_update_draft_only_multiple_times()
    {
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        await UpdateTextAsync(Ids1[0], "iv", "Version2");
        await UpdateTextAsync(Ids1[0], "iv", "Version3");

        await SearchText(expected: null, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version3", target: SearchScope.All);
        await SearchText(expected: null, text: "Version3", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_also_serve_published_after_publish()
    {
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        await PublishAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_also_update_published_content()
    {
        // Create initial content.
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        // Publish the content.
        await PublishAsync(Ids1[0]);

        // Update the published content once.
        await UpdateTextAsync(Ids1[0], "iv", "Version2");

        await SearchText(expected: null, text: "Version1", target: SearchScope.All);
        await SearchText(expected: null, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_also_update_published_content_multiple_times()
    {
        // Create initial content.
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        // Publish the content.
        await PublishAsync(Ids1[0]);

        // Update the published content twice.
        await UpdateTextAsync(Ids1[0], "iv", "Version2");
        await UpdateTextAsync(Ids1[0], "iv", "Version3");

        await SearchText(expected: null, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version3", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version3", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_simulate_new_version()
    {
        // Create initial content.
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        // Publish the content.
        await PublishAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        // Create a new version, the value is still the same as old version.
        await CreateDraftAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        // Make an update, this updates the new version only.
        await UpdateTextAsync(Ids1[0], "iv", "Version2");

        await SearchText(expected: null, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);

        // Publish the new version to get rid of the "Version1" version.
        await PublishAsync(Ids1[0]);

        await SearchText(expected: null, text: "Version1", target: SearchScope.All);
        await SearchText(expected: null, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.Published);

        // Unpublish the version
        await UnpublishAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_simulate_new_version_with_migration()
    {
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        // Publish the content.
        await PublishAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        // Create a new version, his updates the new version also.
        await CreateDraftWithTextAsync(Ids1[0], "iv", "Version2");

        await SearchText(expected: null, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: Ids1, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_simulate_content_reversion()
    {
        await CreateTextAsync(Ids1[0], "iv", "Version1");

        // Publish the content.
        await PublishAsync(Ids1[0]);

        // Create a new version, the value is still the same as old version.
        await CreateDraftAsync(Ids1[0]);

        // Make an update, this updates the new version only.
        await UpdateTextAsync(Ids1[0], "iv", "Version2");

        // Make an update, this updates the new version only.
        await DeleteDraftAsync(Ids1[0]);

        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version1", target: SearchScope.Published);

        await SearchText(expected: null, text: "Version2", target: SearchScope.All);
        await SearchText(expected: null, text: "Version2", target: SearchScope.Published);

        // Make an update, this updates the current version only.
        await UpdateTextAsync(Ids1[0], "iv", "Version3");

        await SearchText(expected: Ids1, text: "Version3", target: SearchScope.All);
        await SearchText(expected: Ids1, text: "Version3", target: SearchScope.Published);
    }

    [Fact]
    public async Task Should_delete_documents_from_index()
    {
        await CreateTextAsync(Ids1[0], "iv", "Text1");
        await CreateTextAsync(Ids2[0], "iv", "Text2");

        await SearchText(expected: Ids1, text: "Text1");
        await SearchText(expected: Ids2, text: "Text2");

        await DeleteAsync(Ids1[0]);

        await SearchText(expected: null, text: "Text1");
        await SearchText(expected: Ids2, text: "Text2");
    }

    [Fact]
    public async Task Should_index_invalid_geodata()
    {
        await CreateGeoAsync(Ids1[0], "field", 144.34, -200);
    }

    [Fact]
    public async Task Should_index_invalid_geojsondata()
    {
        await CreateGeoJsonAsync(Ids1[0], "field", 144.34, -200);
    }

    protected Task CreateTextAsync(DomainId id, string language, string text)
    {
        var data = TextData(language, text);

        return UpdateAsync(id, new ContentCreated { Data = data });
    }

    protected Task CreateGeoAsync(DomainId id, string field, double latitude, double longitude)
    {
        var data = GeoData(field, latitude, longitude);

        return UpdateAsync(id, new ContentCreated { Data = data });
    }

    protected Task CreateGeoJsonAsync(DomainId id, string field, double latitude, double longitude)
    {
        var data = GeoJsonData(field, latitude, longitude);

        return UpdateAsync(id, new ContentCreated { Data = data });
    }

    protected Task UpdateTextAsync(DomainId id, string language, string text)
    {
        var data = TextData(language, text);

        return UpdateAsync(id, new ContentUpdated { Data = data });
    }

    protected Task CreateDraftWithTextAsync(DomainId id, string language, string text)
    {
        var data = TextData(language, text);

        return UpdateAsync(id, new ContentDraftCreated { MigratedData = data });
    }

    protected Task CreateDraftAsync(DomainId id)
    {
        return UpdateAsync(id, new ContentDraftCreated());
    }

    protected Task PublishAsync(DomainId id)
    {
        return UpdateAsync(id, new ContentStatusChanged { Status = Status.Published });
    }

    protected Task UnpublishAsync(DomainId id)
    {
        return UpdateAsync(id, new ContentStatusChanged { Status = Status.Draft });
    }

    protected Task DeleteDraftAsync(DomainId id)
    {
        return UpdateAsync(id, new ContentDraftDeleted());
    }

    protected Task DeleteAsync(DomainId id)
    {
        return UpdateAsync(id, new ContentDeleted());
    }

    private async Task UpdateAsync(DomainId id, ContentEvent contentEvent)
    {
        var sut = await GetProcessAsync();

        contentEvent.ContentId = id;
        contentEvent.AppId = AppId;
        contentEvent.SchemaId = SchemaId;

        await sut.On(Enumerable.Repeat(Envelope.Create<IEvent>(contentEvent), 1));
    }

    private static ContentData TextData(string language, string text)
    {
        return new ContentData()
            .AddField("text",
                new ContentFieldData()
                    .AddLocalized(language, text));
    }

    private static ContentData GeoData(string field, double latitude, double longitude)
    {
        return new ContentData()
            .AddField(field,
                new ContentFieldData()
                    .AddInvariant(JsonValue.Object().Add("latitude", latitude).Add("longitude", longitude)));
    }

    private static ContentData GeoJsonData(string field, double latitude, double longitude)
    {
        return new ContentData()
            .AddField(field,
                new ContentFieldData()
                    .AddInvariant(JsonValue.Object()
                        .Add("type", "Point")
                        .Add("coordinates",
                            new JsonArray()
                                .Add(longitude)
                                .Add(latitude))));
    }

    protected async Task SearchGeo(List<DomainId>? expected, string field, double latitude, double longitude, SearchScope target = SearchScope.All)
    {
        var query = new GeoQuery(SchemaId.Id, field, latitude, longitude, 1000, 1000)
        {
            SchemaId = SchemaId.Id,
        };

        var actual = await SearchAsync(i => i.SearchAsync(App, query, target, default), x => IsExpected(x, expected));
        AssertIds(actual, expected);
    }

    protected async Task SearchText(List<DomainId>? expected, string text, SearchScope target = SearchScope.All)
    {
        var query = new TextQuery(text, 1000)
        {
            RequiredSchemaIds = [SchemaId.Id],
        };

        var actual = await SearchAsync(i => i.SearchAsync(App, query, target, default), x => IsExpected(x, expected));
        AssertIds(actual, expected);
    }

    protected async Task SearchByAppText(List<DomainId>? expected, string text, SearchScope target = SearchScope.All)
    {
        var query = new TextQuery(text, 1000)
        {
            PreferredSchemaId = Schema.Id,
        };

        var actual = await SearchAsync(i => i.SearchAsync(App, query, target, default), x => IsExpected(x, expected));
        AssertIds(actual, expected);
    }

    private async Task<T?> SearchAsync<T>(Func<ITextIndex, Task<T>> query, Predicate<T> isValid)
    {
        var sut = await GetProcessAsync();

        using var cts = new CancellationTokenSource(WaitTime);
        while (!cts.IsCancellationRequested)
        {
            var actual = await query(sut.TextIndex);
            if (isValid(actual))
            {
                return actual;
            }

            await Task.Delay(100, default);
        }

        return default;
    }

    private static bool IsExpected(List<DomainId>? actual, List<DomainId>? expected)
    {
        return expected != null ?
            actual != null && actual.SetEquals(expected) :
            actual == null || actual.Count == 0;
    }

    private static object AssertIds(List<DomainId>? actual, List<DomainId>? expected)
    {
        return expected != null ?
            actual.Should().BeEquivalentTo(expected) :
            actual.Should().BeEmpty();
    }

    private async Task<TextIndexingProcess> GetProcessAsync()
    {
        if (process == null)
        {
            var index = await CreateSutAsync();

            process = new TextIndexingProcess(TestUtils.DefaultSerializer, index, new InMemoryTextIndexerState());
        }

        return process;
    }
}
