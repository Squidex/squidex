/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { SchemasState } from './schemas.state';

import {
    AddFieldDto,
    AppsState,
    AuthService,
    createProperties,
    CreateSchemaDto,
    DateTime,
    DialogService,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaCategoryDto,
    UpdateSchemaDto,
    UpdateSchemaScriptsDto,
    Version,
    Versioned
} from '@app/shared';

describe('SchemasState', () => {
    const app = 'my-app';
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldSchemas = [
        new SchemaDto('id1', 'name1', 'category1', {}, false, creation, creator, creation, creator, version),
        new SchemaDto('id2', 'name2', 'category2', {}, true , creation, creator, creation, creator, version)
    ];

    const nested1 = new NestedFieldDto(3, '3', createProperties('Number'), 2);
    const nested2 = new NestedFieldDto(4, '4', createProperties('String'), 2, true, true);

    const field1 = new RootFieldDto(1, '1', createProperties('String'), 'invariant');
    const field2 = new RootFieldDto(2, '2', createProperties('Array'), 'invariant', true, true, true, [nested1, nested2]);

    const schema =
        new SchemaDetailsDto('id2', 'name2', 'category2', {}, true,
            creation, creator,
            creation, creator,
            version,
            [field1, field2]);

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let authService: IMock<AuthService>;
    let schemasService: IMock<SchemasService>;
    let schemasState: SchemasState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: '1', token: modifier });

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        schemasService = Mock.ofType<SchemasService>();

        schemasService.setup(x => x.getSchemas(app))
            .returns(() => of(oldSchemas));

        schemasService.setup(x => x.getSchema(app, schema.name))
            .returns(() => of(schema));

        schemasService.setup(x => x.getSchema(app, schema.name))
            .returns(() => of(schema));

        schemasState = new SchemasState(appsState.object, authService.object, dialogs.object, schemasService.object);
        schemasState.load().subscribe();
    });

    it('should load schemas', () => {
        expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);
        expect(schemasState.snapshot.isLoaded).toBeTruthy();
        expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false });

        schemasService.verifyAll();
    });

    it('should not remove custom category when loading schemas', () => {
        schemasState.addCategory('category3');
        schemasState.load(true).subscribe();

        expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);
        expect(schemasState.snapshot.isLoaded).toBeTruthy();
        expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false, 'category3': true });

        schemasService.verifyAll();
    });

    it('should show notification on load when reload is true', () => {
        schemasState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add category', () => {
        schemasState.addCategory('category3');

        expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false, 'category3': true });
    });

    it('should remove category', () => {
        schemasState.removeCategory('category1');

        expect(schemasState.snapshot.categories).toEqual({ 'category2': false });
    });

    it('should return schema on select and reload when already loaded', () => {
        schemasState.select('name2').subscribe();
        schemasState.select('name2').subscribe();

        schemasService.verify(x => x.getSchema(app, 'name2'), Times.exactly(2));

        expect().nothing();
    });

    it('should return schema on select and load when not loaded', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.select('name2').subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(<SchemaDetailsDto>schemasState.snapshot.schemas.at(1));
    });

    it('should return null on select  when loading failed', () => {
        schemasService.setup(x => x.getSchema(app, 'failed'))
            .returns(() => throwError({}));

        let selectedSchema: SchemaDetailsDto;

        schemasState.select('failed').subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBeNull();
        expect(schemasState.snapshot.selectedSchema).toBeNull();
    });

    it('should return null on select when unselecting schema', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.select(null).subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBeNull();
        expect(schemasState.snapshot.selectedSchema).toBeNull();

        schemasService.verify(x => x.getSchema(app, It.isAnyString()), Times.never());
    });

    it('should mark published and update user info when published', () => {
        schemasService.setup(x => x.publishSchema(app, oldSchemas[0].name, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        schemasState.publish(oldSchemas[0], modified).subscribe();

        const schema_1 = schemasState.snapshot.schemas.at(0);

        expect(schema_1.isPublished).toBeTruthy();
        expectToBeModified(schema_1);
    });

    it('should unmark published and update user info when unpublished', () => {
        schemasService.setup(x => x.unpublishSchema(app, oldSchemas[1].name, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        schemasState.unpublish(oldSchemas[1], modified).subscribe();

        const schema_1 = schemasState.snapshot.schemas.at(1);

        expect(schema_1.isPublished).toBeFalsy();
        expectToBeModified(schema_1);
    });

    it('should change category and update user info when category changed', () => {
        const category = 'my-new-category';

        schemasService.setup(x => x.putCategory(app, oldSchemas[0].name, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        schemasState.changeCategory(oldSchemas[0], category, modified).subscribe();

        const schema_1 = schemasState.snapshot.schemas.at(0);

        expect(schema_1.category).toEqual(category);
        expectToBeModified(schema_1);
    });

    describe('with selection', () => {
        beforeEach(() => {
            schemasState.select(schema.name).subscribe();
        });

        it('should nmark published and update user info when published selected schema', () => {
            schemasService.setup(x => x.publishSchema(app, schema.name, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.publish(schema, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.isPublished).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should change category and update user info when category of selected schema changed', () => {
            const category = 'my-new-category';

            schemasService.setup(x => x.putCategory(app, oldSchemas[0].name, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.changeCategory(oldSchemas[0], category, modified).subscribe();

            const schema_1 = schemasState.snapshot.schemas.at(0);

            expect(schema_1.category).toEqual(category);
            expectToBeModified(schema_1);
        });

        it('should update properties and update user info when updated', () => {
            const request = new UpdateSchemaDto('name2_label', 'name2_hints');

            schemasService.setup(x => x.putSchema(app, schema.name, It.isAny(), version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.update(schema, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.properties.label).toEqual('name2_label');
            expect(schema_1.properties.hints).toEqual('name2_hints');
            expectToBeModified(schema_1);
        });

        it('should update script properties and update user info when scripts configured', () => {
            const request = new UpdateSchemaScriptsDto('query', 'create', 'update', 'delete', 'change');

            schemasService.setup(x => x.putScripts(app, schema.name, It.isAny(), version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.configureScripts(schema, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.scriptQuery).toEqual('query');
            expect(schema_1.scriptCreate).toEqual('create');
            expect(schema_1.scriptUpdate).toEqual('update');
            expect(schema_1.scriptDelete).toEqual('delete');
            expect(schema_1.scriptChange).toEqual('change');
            expectToBeModified(schema_1);
        });

        it('should add schema to snapshot when created', () => {
            const request = new CreateSchemaDto('newName');

            const result = new SchemaDetailsDto('id4', 'newName', '', {}, false, modified, modifier, modified, modifier, version, []);

            schemasService.setup(x => x.postSchema(app, request, modifier, modified))
                .returns(() => of(result));

            schemasState.create(request, modified).subscribe();

            expect(schemasState.snapshot.schemas.values.length).toBe(3);
            expect(schemasState.snapshot.schemas.at(2)).toBe(result);
        });

        it('should remove schema from snapshot when deleted', () => {
            schemasService.setup(x => x.deleteSchema(app, schema.name, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.delete(schema).subscribe();

            expect(schemasState.snapshot.schemas.values.length).toBe(1);
            expect(schemasState.snapshot.selectedSchema).toBeNull();
        });

        it('should add field and update user info when field added', () => {
            const request = new AddFieldDto(field1.name, field1.partitioning, field1.properties);

            const newField = new RootFieldDto(3, '3', createProperties('String'), 'invariant');

            schemasService.setup(x => x.postField(app, schema.name, It.isAny(), undefined, version))
                .returns(() => of(new Versioned<RootFieldDto>(newVersion, newField)));

            schemasState.addField(schema, request, undefined, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field1, field2, newField]);
            expectToBeModified(schema_1);
        });

        it('should add field and update user info when nested field added', () => {
            const request = new AddFieldDto(field1.name, field1.partitioning, field1.properties);

            const newField = new NestedFieldDto(3, '3', createProperties('String'), 2);

            schemasService.setup(x => x.postField(app, schema.name, It.isAny(), 2, version))
                .returns(() => of(new Versioned<NestedFieldDto>(newVersion, newField)));

            schemasState.addField(schema, request, field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested).toEqual([nested1, nested2, newField]);
            expectToBeModified(schema_1);
        });

        it('should remove field and update user info when field removed', () => {
            schemasService.setup(x => x.deleteField(app, schema.name, field1.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.deleteField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field2]);
            expectToBeModified(schema_1);
        });

        it('should remove field and update user info when nested field removed', () => {
            schemasService.setup(x => x.deleteField(app, schema.name, nested1.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.deleteField(schema, nested1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested).toEqual([nested2]);
            expectToBeModified(schema_1);
        });

        it('should sort fields and update user info when fields sorted', () => {
            schemasService.setup(x => x.putFieldOrdering(app, schema.name, [field2.fieldId, field1.fieldId], undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.sortFields(schema, [field2, field1], undefined, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field2, field1]);
            expectToBeModified(schema_1);
        });

        it('should sort fields and update user info when nested fields sorted', () => {
            schemasService.setup(x => x.putFieldOrdering(app, schema.name, [nested2.fieldId, nested1.fieldId], 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.sortFields(schema, [nested2, nested1], field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested).toEqual([nested2, nested1]);
            expectToBeModified(schema_1);
        });

        it('should update field properties and update user info when field updated', () => {
            const request = new UpdateFieldDto(createProperties('String'));

            schemasService.setup(x => x.putField(app, schema.name, field1.fieldId, request, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.updateField(schema, field1, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].properties).toBe(request.properties);
            expectToBeModified(schema_1);
        });

        it('should update field properties and update user info when nested field updated', () => {
            const request = new UpdateFieldDto(createProperties('String'));

            schemasService.setup(x => x.putField(app, schema.name, nested1.fieldId, request, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.updateField(schema, nested1, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[0].properties).toBe(request.properties);
            expectToBeModified(schema_1);
        });

        it('should mark field hidden and update user info when field hidden', () => {
            schemasService.setup(x => x.hideField(app, schema.name, field1.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.hideField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isHidden).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should mark field hidden and update user info when nested field hidden', () => {
            schemasService.setup(x => x.hideField(app, schema.name, nested1.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.hideField(schema, nested1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[0].isHidden).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should mark field disabled and update user info when field disabled', () => {
            schemasService.setup(x => x.disableField(app, schema.name, field1.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.disableField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isDisabled).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should mark field disabled and update user info when nested disabled', () => {
            schemasService.setup(x => x.disableField(app, schema.name, nested1.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.disableField(schema, nested1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[0].isDisabled).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should mark field locked and update user info when field locked', () => {
            schemasService.setup(x => x.lockField(app, schema.name, field1.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.lockField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isLocked).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should mark field locked and update user info when nested field locked', () => {
            schemasService.setup(x => x.lockField(app, schema.name, nested1.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.lockField(schema, nested1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[0].isLocked).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should unmark field hidden and update user info when field shown', () => {
            schemasService.setup(x => x.showField(app, schema.name, field2.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.showField(schema, field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].isHidden).toBeFalsy();
            expectToBeModified(schema_1);
        });

        it('should unmark field hidden and update user info when nested field shown', () => {
            schemasService.setup(x => x.showField(app, schema.name, nested2.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.showField(schema, nested2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[1].isHidden).toBeFalsy();
            expectToBeModified(schema_1);
        });

        it('should unmark field disabled and update user info when field enabled', () => {
            schemasService.setup(x => x.enableField(app, schema.name, field2.fieldId, undefined, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.enableField(schema, field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].isDisabled).toBeFalsy();
            expectToBeModified(schema_1);
        });

        it('should unmark field disabled and update user info when nested field enabled', () => {
            schemasService.setup(x => x.enableField(app, schema.name, nested2.fieldId, 2, version))
                .returns(() => of(new Versioned<any>(newVersion, {})));

            schemasState.enableField(schema, nested2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].nested[1].isDisabled).toBeFalsy();
            expectToBeModified(schema_1);
        });
    });

    function expectToBeModified(schema_1: SchemaDto) {
        expect(schema_1.lastModified).toEqual(modified);
        expect(schema_1.lastModifiedBy).toEqual(modifier);
        expect(schema_1.version).toEqual(newVersion);
    }
});