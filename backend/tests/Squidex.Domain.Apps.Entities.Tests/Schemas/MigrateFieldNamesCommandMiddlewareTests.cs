// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class MigrateFieldNamesCommandMiddlewareTests : GivenContext
{
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly MigrateFieldNamesCommandMiddleware sut;

    public MigrateFieldNamesCommandMiddlewareTests()
    {
        sut = new MigrateFieldNamesCommandMiddleware();
    }

    [Fact]
    public async Task Should_migrate_synchronize_command()
    {
        var command = new SynchronizeSchema
        {
            FieldsInLists = FieldNames.Create(
                "meta.id",
                "meta.lastModified",
                "dataField1"
            ),
            FieldsInReferences = FieldNames.Create(
                "meta.version",
                "meta.lastModifiedBy",
                "dataField2"
            ),
        };

        var commandContext = new CommandContext(command, commandBus);

        await sut.HandleAsync(commandContext, CancellationToken);

        Assert.Equal(FieldNames.Create("id", "lastModified", "data.dataField1"),
            command.FieldsInLists);

        Assert.Equal(FieldNames.Create("version", "lastModifiedBy", "data.dataField2"),
            command.FieldsInReferences);
    }

    [Fact]
    public async Task Should_migrate_synchronize_command_with_null_fields()
    {
        var command = new SynchronizeSchema();

        var commandContext = new CommandContext(command, commandBus);

        await sut.HandleAsync(commandContext, CancellationToken);

        Assert.Null(command.FieldsInLists);
        Assert.Null(command.FieldsInReferences);
    }

    [Fact]
    public async Task Should_migrate_configure_command()
    {
        var command = new ConfigureUIFields
        {
            FieldsInLists = FieldNames.Create(
                "meta.id",
                "meta.lastModified",
                "dataField1"
            ),
            FieldsInReferences = FieldNames.Create(
                "meta.version",
                "meta.lastModifiedBy",
                "dataField2"
            ),
        };

        var commandContext = new CommandContext(command, commandBus);

        await sut.HandleAsync(commandContext, CancellationToken);

        Assert.Equal(FieldNames.Create("id", "lastModified", "data.dataField1"),
            command.FieldsInLists);

        Assert.Equal(FieldNames.Create("version", "lastModifiedBy", "data.dataField2"),
            command.FieldsInReferences);
    }

    [Fact]
    public async Task Should_migrate_configure_command_with_null_fields()
    {
        var command = new ConfigureUIFields();

        var commandContext = new CommandContext(command, commandBus);

        await sut.HandleAsync(commandContext, CancellationToken);

        Assert.Null(command.FieldsInLists);
        Assert.Null(command.FieldsInReferences);
    }
}
