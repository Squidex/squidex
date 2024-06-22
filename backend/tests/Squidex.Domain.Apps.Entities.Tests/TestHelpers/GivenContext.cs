// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Extensions.Actions.Webhook;
using Squidex.Infrastructure;
using ContentFieldData = Squidex.Domain.Apps.Core.Contents.ContentFieldData;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public abstract class GivenContext
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private IAppProvider? appProvider;
    private IContextProvider? contextProviderApi;
    private IContextProvider? contextProviderFrontend;
    private Context? contextApi;
    private Context? contextFrontend;

    public DomainId TeamId { get; } = DomainId.NewGuid();

    public string TeamName { get; } = "my-team";

    public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

    public NamedId<DomainId> SchemaId { get; } = NamedId.Of(DomainId.NewGuid(), "my-schema");

    public App App { get; set; }

    public Team Team { get; set; }

    public Schema Schema { get; set; }

    public RefToken User { get; } = RefToken.User("me");

    public RefToken Client { get; } = RefToken.Client("client");

    public Context ApiContext
    {
        get => contextApi ??= new Context(Mocks.ApiUser(), App);
    }

    public Context FrontendContext
    {
        get => contextFrontend ??= new Context(Mocks.FrontendUser(), App);
    }

    public IContextProvider ApiContextProvider
    {
        get => contextProviderApi ??= CreateContextProvider(ApiContext);
    }

    public IContextProvider FrontendContextProvider
    {
        get => contextProviderFrontend ??= CreateContextProvider(FrontendContext);
    }

    public IAppProvider AppProvider
    {
        get => appProvider ??= CreateAppProvider();
    }

    public CancellationToken CancellationToken => cts.Token;

    protected GivenContext()
    {
        Team = new Team
        {
            Id = TeamId,
            Name = TeamName,
            Created = default,
            CreatedBy = User,
            LastModified = default,
            LastModifiedBy = User,
            Version = 1,
        };

        App = new App
        {
            Id = AppId.Id,
            Name = AppId.Name,
            Created = default,
            CreatedBy = User,
            Languages = LanguagesConfig.English.Set(Language.DE, false),
            LastModified = default,
            LastModifiedBy = User,
            TeamId = TeamId,
            Version = 1,
        };

        Schema = new Schema
        {
            Id = SchemaId.Id,
            Name = SchemaId.Name,
            AppId = AppId,
            Created = default,
            CreatedBy = User,
            IsPublished = true,
            LastModified = default,
            LastModifiedBy = User,
            Version = 1,
        };
    }

    public static Instant Timestamp()
    {
        return SystemClock.Instance.GetCurrentInstant();
    }

    public Context CreateContext(params string[] permissions)
    {
        var principal = Mocks.CreateUser(false, null, permissions);

        return new Context(principal, App);
    }

    public Context CreateContext(bool isFrontend, params string[] permissions)
    {
        var principal = Mocks.CreateUser(isFrontend, null, permissions);

        return new Context(principal, App);
    }

    private static IContextProvider CreateContextProvider(Context context)
    {
        var result = A.Fake<IContextProvider>();

        A.CallTo(() => result.Context)
            .Returns(context);

        return result;
    }

    private IAppProvider CreateAppProvider()
    {
        var result = A.Fake<IAppProvider>();

        A.CallTo(() => result.GetAppAsync(AppId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => App);

        A.CallTo(() => result.GetTeamAsync(TeamId, A<CancellationToken>._))
            .ReturnsLazily(() => Team);

        A.CallTo(() => result.GetSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => Schema);

        A.CallTo(() => result.GetSchemaAsync(AppId.Id, SchemaId.Name, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => Schema);

        A.CallTo(() => result.GetSchemasAsync(AppId.Id, A<CancellationToken>._))
            .ReturnsLazily(() => [Schema]);

        A.CallTo(() => result.GetAppWithSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => (App, Schema));

        return result;
    }

    public EnrichedAsset CreateAsset()
    {
        var id = DomainId.NewGuid();

        return new EnrichedAsset
        {
            Id = id,
            AppId = AppId,
            Created = Timestamp(),
            CreatedBy = User,
            FileName = "My File.png",
            FileHash = "my-hash-42",
            FileSize = 1024,
            FileVersion = 0,
            TotalSize = 1024 * 2,
            LastModified = Timestamp(),
            LastModifiedBy = User,
            Metadata = [],
            MetadataText = string.Empty,
            MimeType = "image/png",
            Tags = [],
            TagNames = [],
            Slug = "my-file",
            Version = 1,
        };
    }

    public AssetFolder CreateAssetFolder()
    {
        var id = DomainId.NewGuid();

        return new AssetFolder
        {
            Id = id,
            AppId = AppId,
            Created = Timestamp(),
            CreatedBy = User,
            FolderName = "My Folder",
            LastModified = Timestamp(),
            LastModifiedBy = User,
            Version = 1,
        };
    }

    public EnrichedRule CreateRule()
    {
        var id = DomainId.NewGuid();

        return new EnrichedRule
        {
            Id = id,
            AppId = AppId,
            Action = new WebhookAction(),
            Created = Timestamp(),
            CreatedBy = User,
            Name = "My Rule",
            LastModified = Timestamp(),
            LastModifiedBy = User,
            Trigger = new ContentChangedTriggerV2(),
            Version = 1,
        };
    }

    public EnrichedContent CreateContent()
    {
        var id = DomainId.NewGuid();

        var data =
            new ContentData()
                .AddField("my-field",
                    new ContentFieldData()
                        .AddInvariant(42));

        return new EnrichedContent
        {
            Id = id,
            AppId = AppId,
            Created = Timestamp(),
            CreatedBy = User,
            Data = data,
            LastModified = Timestamp(),
            LastModifiedBy = User,
            ScheduleJob = new ScheduleJob(DomainId.NewGuid(), Status.Archived, User, Timestamp()),
            SchemaId = SchemaId,
            Status = Status.Published,
            Version = 1,
        };
    }

    public WriteContent CreateWriteContent()
    {
        var id = DomainId.NewGuid();

        var data =
            new ContentData()
                .AddField("my-field",
                    new ContentFieldData()
                        .AddInvariant(42));

        return new WriteContent
        {
            Id = id,
            AppId = AppId,
            Created = Timestamp(),
            CreatedBy = User,
            CurrentVersion = new ContentVersion(Status.Published, data),
            NewVersion = null,
            LastModified = Timestamp(),
            LastModifiedBy = User,
            ScheduleJob = new ScheduleJob(DomainId.NewGuid(), Status.Archived, User, Timestamp()),
            SchemaId = SchemaId,
            Version = 1,
        };
    }
}
