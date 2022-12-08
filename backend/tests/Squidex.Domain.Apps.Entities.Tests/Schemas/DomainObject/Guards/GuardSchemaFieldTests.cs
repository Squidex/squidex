// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards;

public class GuardSchemaFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly Schema schema_0;
    private readonly StringFieldProperties validProperties = new StringFieldProperties();
    private readonly StringFieldProperties invalidProperties = new StringFieldProperties { MinLength = 10, MaxLength = 5 };

    public GuardSchemaFieldTests()
    {
        schema_0 =
            new Schema("my-schema")
                .AddString(1, "field1", Partitioning.Invariant)
                .AddString(2, "field2", Partitioning.Invariant)
                .AddArray(3, "field3", Partitioning.Invariant, f => f
                    .AddNumber(301, "field301"))
                .AddUI(4, "field4", Partitioning.Invariant);
    }

    private static Action<T, Schema> A<T>(Action<T, Schema> method) where T : FieldCommand
    {
        return method;
    }

    public static IEnumerable<object[]> FieldCommandData()
    {
        yield return new object[] { A<EnableField>(GuardSchemaField.CanEnable) };
        yield return new object[] { A<DeleteField>(GuardSchemaField.CanDelete) };
        yield return new object[] { A<DisableField>(GuardSchemaField.CanDisable) };
        yield return new object[] { A<HideField>(GuardSchemaField.CanHide) };
        yield return new object[] { A<LockField>(GuardSchemaField.CanLock) };
        yield return new object[] { A<ShowField>(GuardSchemaField.CanShow) };
        yield return new object[] { A<UpdateField>(GuardSchemaField.CanUpdate) };
    }

    [Theory]
    [MemberData(nameof(FieldCommandData))]
    public void Commands_should_throw_exception_if_field_not_found<T>(Action<T, Schema> action) where T : FieldCommand, new()
    {
        var command = new T { FieldId = 5 };

        Assert.Throws<DomainObjectNotFoundException>(() => action(command, schema_0));
    }

    [Theory]
    [MemberData(nameof(FieldCommandData))]
    public void Commands_should_throw_exception_if_parent_field_not_found<T>(Action<T, Schema> action) where T : FieldCommand, new()
    {
        var command = new T { ParentFieldId = 4, FieldId = 401 };

        Assert.Throws<DomainObjectNotFoundException>(() => action(command, schema_0));
    }

    [Theory]
    [MemberData(nameof(FieldCommandData))]
    public void Commands_should_throw_exception_if_child_field_not_found<T>(Action<T, Schema> action) where T : FieldCommand, new()
    {
        var command = new T { ParentFieldId = 3, FieldId = 302 };

        Assert.Throws<DomainObjectNotFoundException>(() => action(command, schema_0));
    }

    [Fact]
    public void CanDisable_should_not_throw_exception_if_already_disabled()
    {
        var command = new DisableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Disable());

        GuardSchemaField.CanDisable(command, schema_1);
    }

    [Fact]
    public void CanDisable_should_throw_exception_if_locked()
    {
        var command = new DisableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanDisable(command, schema_1));
    }

    [Fact]
    public void CanDisable_should_throw_exception_if_ui_field()
    {
        var command = new DisableField { FieldId = 4 };

        Assert.Throws<DomainException>(() => GuardSchemaField.CanDisable(command, schema_0));
    }

    [Fact]
    public void CanEnable_should_not_throw_exception_if_already_enabled()
    {
        var command = new EnableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Enable());

        GuardSchemaField.CanEnable(command, schema_1);
    }

    [Fact]
    public void CanEnable_should_throw_exception_if_locked()
    {
        var command = new EnableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanEnable(command, schema_1));
    }

    [Fact]
    public void CanEnable_should_throw_exception_if_ui_field()
    {
        var command = new EnableField { FieldId = 4 };

        Assert.Throws<DomainException>(() => GuardSchemaField.CanEnable(command, schema_0));
    }

    [Fact]
    public void CanHide_should_not_throw_exception_if_already_hidden()
    {
        var command = new EnableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Hide());

        GuardSchemaField.CanEnable(command, schema_1);
    }

    [Fact]
    public void CanHide_should_throw_exception_if_locked()
    {
        var command = new HideField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanHide(command, schema_1));
    }

    [Fact]
    public void CanHide_should_throw_exception_if_ui_field()
    {
        var command = new HideField { FieldId = 4 };

        Assert.Throws<DomainException>(() => GuardSchemaField.CanHide(command, schema_0));
    }

    [Fact]
    public void CanShow_should_not_throw_exception_if_already_shown()
    {
        var command = new EnableField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Show());

        GuardSchemaField.CanEnable(command, schema_1);
    }

    [Fact]
    public void CanShow_should_throw_exception_if_locked()
    {
        var command = new ShowField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanShow(command, schema_1));
    }

    [Fact]
    public void CanShow_should_throw_exception_if_ui_field()
    {
        var command = new ShowField { FieldId = 4 };

        Assert.Throws<DomainException>(() => GuardSchemaField.CanShow(command, schema_0));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_locked()
    {
        var command = new DeleteField { FieldId = 1 };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanDelete(command, schema_1));
    }

    [Fact]
    public void CanDelete_should_not_throw_exception_if_not_locked()
    {
        var command = new DeleteField { FieldId = 1 };

        GuardSchemaField.CanDelete(command, schema_0);
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_locked()
    {
        var command = new UpdateField { FieldId = 1, Properties = validProperties };

        var schema_1 = schema_0.UpdateField(1, f => f.Lock());

        Assert.Throws<DomainException>(() => GuardSchemaField.CanUpdate(command, schema_1));
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_not_locked()
    {
        var command = new UpdateField { FieldId = 1, Properties = validProperties };

        GuardSchemaField.CanUpdate(command, schema_0);
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_properties_null()
    {
        var command = new UpdateField { FieldId = 2, Properties = null! };

        ValidationAssert.Throws(() => GuardSchemaField.CanUpdate(command, schema_0),
            new ValidationError("Properties is required.", "Properties"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_properties_not_valid()
    {
        var command = new UpdateField { FieldId = 2, Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 } };

        ValidationAssert.Throws(() => GuardSchemaField.CanUpdate(command, schema_0),
            new ValidationError("Max length must be greater or equal to min length.", "Properties.MinLength", "Properties.MaxLength"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_field_already_exists()
    {
        var command = new AddField { Name = "field1", Properties = validProperties };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("A field with the same name already exists."));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_nested_field_already_exists()
    {
        var command = new AddField { Name = "field301", Properties = validProperties, ParentFieldId = 3 };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("A field with the same name already exists."));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_name_not_valid()
    {
        var command = new AddField { Name = "INVALID_NAME", Properties = validProperties };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("Name is not a Javascript property name.", "Name"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_properties_not_valid()
    {
        var command = new AddField { Name = "field5", Properties = invalidProperties };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("Max length must be greater or equal to min length.", "Properties.MinLength", "Properties.MaxLength"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_properties_null()
    {
        var command = new AddField { Name = "field5", Properties = null! };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("Properties is required.", "Properties"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_partitioning_not_valid()
    {
        var command = new AddField { Name = "field5", Partitioning = "INVALID_PARTITIONING", Properties = validProperties };

        ValidationAssert.Throws(() => GuardSchemaField.CanAdd(command, schema_0),
            new ValidationError("Partitioning is not a valid value.", "Partitioning"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_parent_not_exists()
    {
        var command = new AddField { Name = "field302", Properties = validProperties, ParentFieldId = 99 };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanAdd(command, schema_0));
    }

    [Fact]
    public void CanAdd_should_not_throw_exception_if_field_not_exists()
    {
        var command = new AddField { Name = "field5", Properties = validProperties };

        GuardSchemaField.CanAdd(command, schema_0);
    }

    [Fact]
    public void CanAdd_should_not_throw_exception_if_field_exists_in_root()
    {
        var command = new AddField { Name = "field1", Properties = validProperties, ParentFieldId = 3 };

        GuardSchemaField.CanAdd(command, schema_0);
    }
}