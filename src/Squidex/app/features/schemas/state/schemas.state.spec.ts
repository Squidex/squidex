/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { AuthService, DialogService, Versioned, AddFieldDto, CreateSchemaDto, UpdateFieldDto } from '@app/shared';

import { SchemasState } from './schemas.state';

import {
    AppsState,
    createProperties,
    DateTime,
    FieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    UpdateSchemaDto,
    UpdateSchemaScriptsDto,
    Version
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
        new SchemaDto('id1', 'name1', {}, false, creator, creator, creation, creation, version),
        new SchemaDto('id2', 'name2', {}, true , creator, creator, creation, creation, version)
    ];

    const field1 = new FieldDto(1, '1', false, false, false, 'l', createProperties('String'));
    const field2 = new FieldDto(2, '2', true,  true,  true,  'l', createProperties('Number'));

    const schema =
        new SchemaDetailsDto('id2', 'name2', {}, true,
            creator, creator,
            creation, creation,
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
            .returns(() => Observable.of(oldSchemas));

        schemasService.setup(x => x.getSchema(app, schema.name))
            .returns(() => Observable.of(schema));

        schemasService.setup(x => x.getSchema(app, schema.name))
            .returns(() => Observable.of(schema));

        schemasState = new SchemasState(appsState.object, authService.object, dialogs.object, schemasService.object);
        schemasState.loadSchemas().subscribe();
    });

    it('should load schemas', () => {
        expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);

        schemasService.verifyAll();
    });

    it('should reload schema when already loaded', () => {
        schemasState.selectSchema('name2').subscribe();
        schemasState.selectSchema('name2').subscribe();

        schemasService.verify(x => x.getSchema(app, 'name2'), Times.exactly(2));
    });

    it('should load selected schema when not loaded', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.selectSchema('name2').subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(schemasState.snapshot.schemas.at(1));
    });

    it('should return null when loading failed', () => {
        schemasService.setup(x => x.getSchema(app, 'failed'))
            .returns(() => Observable.throw({}));

        let selectedSchema: SchemaDetailsDto;

        schemasState.selectSchema('failed').subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBeNull();
        expect(schemasState.snapshot.selectedSchema).toBeNull();
    });

    it('should return null when unselecting schema', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.selectSchema(null).subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBeNull();
        expect(schemasState.snapshot.selectedSchema).toBeNull();

        schemasService.verify(x => x.getSchema(app, It.isAnyString()), Times.never());
    });

    it('should update isPublished property and user info when publishing', () => {
        schemasService.setup(x => x.publishSchema(app, oldSchemas[0].name, version))
            .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

        schemasState.publish(oldSchemas[0], modified).subscribe();

        const schema_1 = schemasState.snapshot.schemas.at(0);

        expect(schema_1.isPublished).toBeTruthy();
        expectToBeModified(schema_1);
    });

    it('should update isPublished property and user info when unpublishing', () => {
        schemasService.setup(x => x.unpublishSchema(app, oldSchemas[1].name, version))
            .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

        schemasState.unpublish(oldSchemas[1], modified).subscribe();

        const schema_1 = schemasState.snapshot.schemas.at(1);

        expect(schema_1.isPublished).toBeFalsy();
        expectToBeModified(schema_1);
    });

    describe('without selection', () => {
        beforeEach(() => {
            schemasState.selectSchema(schema.name).subscribe();
        });

        it('should update isPublished property and user info when publishing selected schema', () => {
            schemasService.setup(x => x.publishSchema(app, schema.name, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.publish(schema, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.isPublished).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should update properties property and user info when updating', () => {
            const request = new UpdateSchemaDto('name2_label', 'name2_hints');

            schemasService.setup(x => x.putSchema(app, schema.name, It.isAny(), version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.update(schema, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.properties.label).toEqual('name2_label');
            expect(schema_1.properties.hints).toEqual('name2_hints');
            expectToBeModified(schema_1);
        });

        it('should update script properties and user info when updating', () => {
            const request = new UpdateSchemaScriptsDto('query', 'create', 'update', 'delete', 'change');

            schemasService.setup(x => x.putSchemaScripts(app, schema.name, It.isAny(), version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.configureScripts(schema, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.scriptQuery).toEqual('query');
            expect(schema_1.scriptCreate).toEqual('create');
            expect(schema_1.scriptUpdate).toEqual('update');
            expect(schema_1.scriptDelete).toEqual('delete');
            expect(schema_1.scriptChange).toEqual('change');
            expectToBeModified(schema_1);
        });

        it('should add schema when created', () => {
            const request = new CreateSchemaDto('newName');

            const result = new SchemaDetailsDto('id4', 'newName', {}, false, modifier, modifier, modified, modified, version, []);

            schemasService.setup(x => x.postSchema(app, request, modifier, modified))
                .returns(() => Observable.of(result));

            schemasState.create(request, modified).subscribe();

            expect(schemasState.snapshot.schemas.values.length).toBe(3);
            expect(schemasState.snapshot.schemas.at(2)).toBe(result);
        });

        it('should remove schema when deleted', () => {
            schemasService.setup(x => x.deleteSchema(app, schema.name, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.delete(schema).subscribe();

            expect(schemasState.snapshot.schemas.values.length).toBe(1);
            expect(schemasState.snapshot.selectedSchema).toBeNull();
        });

        it('should update fields property and user info when adding field', () => {
            const request = new AddFieldDto(field1.name, field1.partitioning, field1.properties);

            const newField = new FieldDto(3, '3', false, false, false, 'l', createProperties('String'));

            schemasService.setup(x => x.postField(app, schema.name, It.isAny(), version))
                .returns(() => Observable.of(new Versioned<FieldDto>(newVersion, newField)));

            schemasState.addField(schema, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field1, field2, newField]);
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when removing field', () => {
            schemasService.setup(x => x.deleteField(app, schema.name, field1.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.deleteField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field2]);
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when sorting fields', () => {
            schemasService.setup(x => x.putFieldOrdering(app, schema.name, [field2.fieldId, field1.fieldId], version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.sortFields(schema, [field2, field1], modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields).toEqual([field2, field1]);
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when updating field', () => {
            const request = new UpdateFieldDto(createProperties('String'));

            schemasService.setup(x => x.putField(app, schema.name, field1.fieldId, request, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.updateField(schema, field1, request, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].properties).toBe(request.properties);
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when hiding field', () => {
            schemasService.setup(x => x.hideField(app, schema.name, field1.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.hideField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isHidden).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when disabling field', () => {
            schemasService.setup(x => x.disableField(app, schema.name, field1.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.disableField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isDisabled).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when locking field', () => {
            schemasService.setup(x => x.lockField(app, schema.name, field1.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.lockField(schema, field1, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[0].isLocked).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when showing field', () => {
            schemasService.setup(x => x.showField(app, schema.name, field2.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.showField(schema, field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].isHidden).toBeFalsy();
            expectToBeModified(schema_1);
        });

        it('should update fields property and user info when enabling field', () => {
            schemasService.setup(x => x.enableField(app, schema.name, field2.fieldId, version))
                .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

            schemasState.enableField(schema, field2, modified).subscribe();

            const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

            expect(schema_1.fields[1].isDisabled).toBeFalsy();
            expectToBeModified(schema_1);
        });
    });

    function expectToBeModified(schema_1: SchemaDto) {
        expect(schema_1.lastModified).toEqual(modified);
        expect(schema_1.lastModifiedBy).toEqual(modifier);
        expect(schema_1.version).toEqual(newVersion);
    }
});