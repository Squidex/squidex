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
    createProperties,
    CreateSchemaDto,
    DialogService,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    UpdateSchemaCategoryDto,
    versioned
} from '@app/shared/internal';

import { TestValues } from './_test-helpers';

describe('SchemasState', () => {
    const {
        app,
        appsState,
        authService,
        creation,
        creator,
        modified,
        modifier,
        newVersion,
        version
    } = TestValues;

    const oldSchemas = [
        new SchemaDto('id1', 'name1', 'category1', {}, false, false, creation, creator, creation, creator, version),
        new SchemaDto('id2', 'name2', 'category2', {}, false, true , creation, creator, creation, creator, version)
    ];

    const nested1 = new NestedFieldDto(3, '3', createProperties('Number'), 2);
    const nested2 = new NestedFieldDto(4, '4', createProperties('String'), 2, true, true);

    const field1 = new RootFieldDto(1, '1', createProperties('String'), 'invariant');
    const field2 = new RootFieldDto(2, '2', createProperties('Array'), 'invariant', true, true, true, [nested1, nested2]);

    const schema =
        new SchemaDetailsDto('id2', 'name2', 'category2', {}, false, true,
            creation, creator,
            creation, creator,
            version,
            [field1, field2]);

    let dialogs: IMock<DialogService>;
    let schemasService: IMock<SchemasService>;
    let schemasState: SchemasState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        schemasService = Mock.ofType<SchemasService>();
        schemasState = new SchemasState(appsState.object, authService.object, dialogs.object, schemasService.object);
    });

    afterEach(() => {
        schemasService.verifyAll();
    });

    describe('Loading', () => {
        it('should load schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load().subscribe();

            expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);
            expect(schemasState.snapshot.isLoaded).toBeTruthy();
            expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false, '': true });

            schemasService.verifyAll();
        });

        it('should not remove custom category when loading schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.addCategory('category3');
            schemasState.load(true).subscribe();

            expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);
            expect(schemasState.snapshot.isLoaded).toBeTruthy();
            expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false, 'category3': true, '': true });

            schemasService.verifyAll();
        });

        it('should show notification on load when reload is true', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load().subscribe();
        });

        it('should add category', () => {
            schemasState.addCategory('category3');

            expect(schemasState.snapshot.categories).toEqual({ 'category1': false, 'category2': false, 'category3': true, '': true });
        });

        it('should remove category', () => {
            schemasState.removeCategory('category1');

            expect(schemasState.snapshot.categories).toEqual({ 'category2': false, '': true });
        });

        it('should return schema on select and reload when already loaded', () => {
            schemasService.setup(x => x.getSchema(app, schema.name))
                .returns(() => of(schema)).verifiable(Times.exactly(2));

            schemasState.select('name2').subscribe();
            schemasState.select('name2').subscribe();

            expect().nothing();
        });

        it('should return schema on select and load when not loaded', () => {
            schemasService.setup(x => x.getSchema(app, schema.name))
                .returns(() => of(schema)).verifiable();

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
                .returns(() => throwError({})).verifiable();

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
        });

        it('should mark published and update user info when published', () => {
            schemasService.setup(x => x.publishSchema(app, oldSchemas[0].name, version))
                .returns(() => of(versioned(newVersion))).verifiable();

            schemasState.publish(oldSchemas[0], modified).subscribe();

            const schema_1 = schemasState.snapshot.schemas.at(0);

            expect(schema_1.isPublished).toBeTruthy();
            expectToBeModified(schema_1);
        });

        it('should unmark published and update user info when unpublished', () => {
            schemasService.setup(x => x.unpublishSchema(app, oldSchemas[1].name, version))
                .returns(() => of(versioned(newVersion))).verifiable();

            schemasState.unpublish(oldSchemas[1], modified).subscribe();

            const schema_1 = schemasState.snapshot.schemas.at(1);

            expect(schema_1.isPublished).toBeFalsy();
            expectToBeModified(schema_1);
        });

        it('should change category and update user info when category changed', () => {
            const category = 'my-new-category';

            schemasService.setup(x => x.putCategory(app, oldSchemas[0].name, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                .returns(() => of(versioned(newVersion))).verifiable();

            schemasState.changeCategory(oldSchemas[0], category, modified).subscribe();

            const schema_1 = schemasState.snapshot.schemas.at(0);

            expect(schema_1.category).toEqual(category);
            expectToBeModified(schema_1);
        });

        describe('with selection', () => {
            beforeEach(() => {
                schemasService.setup(x => x.getSchema(app, schema.name))
                    .returns(() => of(schema)).verifiable();

                schemasState.select(schema.name).subscribe();
            });

            it('should nmark published and update user info when published selected schema', () => {
                schemasService.setup(x => x.publishSchema(app, schema.name, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.publish(schema, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.isPublished).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should change category and update user info when category of selected schema changed', () => {
                const category = 'my-new-category';

                schemasService.setup(x => x.putCategory(app, oldSchemas[0].name, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.changeCategory(oldSchemas[0], category, modified).subscribe();

                const schema_1 = schemasState.snapshot.schemas.at(0);

                expect(schema_1.category).toEqual(category);
                expectToBeModified(schema_1);
            });

            it('should update properties and update user info when updated', () => {
                const request = { label: 'name2_label', hints: 'name2_hints' };

                schemasService.setup(x => x.putSchema(app, schema.name, It.isAny(), version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.update(schema, request, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.properties.label).toEqual(request.label);
                expect(schema_1.properties.hints).toEqual(request.hints);
                expectToBeModified(schema_1);
            });

            it('should update script properties and update user info when scripts configured', () => {
                const request = { query: '<query-script>' };

                schemasService.setup(x => x.putScripts(app, schema.name, It.isAny(), version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.configureScripts(schema, request, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.scripts['query']).toEqual('<query-script>');
                expectToBeModified(schema_1);
            });

            it('should update script properties and update user info when preview urls configured', () => {
                const request = { web: 'url' };

                schemasService.setup(x => x.putPreviewUrls(app, schema.name, It.isAny(), version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.configurePreviewUrls(schema, request, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.previewUrls).toEqual(request);
                expectToBeModified(schema_1);
            });

            it('should add schema to snapshot when created', () => {
                const request = new CreateSchemaDto('newName');

                const result = new SchemaDetailsDto('id4', 'newName', '', {}, false, false, modified, modifier, modified, modifier, version);

                schemasService.setup(x => x.postSchema(app, request, modifier, modified))
                    .returns(() => of(result)).verifiable();

                schemasState.create(request, modified).subscribe();

                expect(schemasState.snapshot.schemas.values.length).toBe(3);
                expect(schemasState.snapshot.schemas.at(2)).toBe(result);
            });

            it('should remove schema from snapshot when deleted', () => {
                schemasService.setup(x => x.deleteSchema(app, schema.name, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.delete(schema).subscribe();

                expect(schemasState.snapshot.schemas.values.length).toBe(1);
                expect(schemasState.snapshot.selectedSchema).toBeNull();
            });

            it('should add field and update user info when field added', () => {
                const request = new AddFieldDto(field1.name, field1.partitioning, field1.properties);

                const newField = new RootFieldDto(3, '3', createProperties('String'), 'invariant');

                schemasService.setup(x => x.postField(app, schema.name, It.isAny(), undefined, version))
                    .returns(() => of(versioned(newVersion, newField))).verifiable();

                schemasState.addField(schema, request, undefined, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields).toEqual([field1, field2, newField]);
                expectToBeModified(schema_1);
            });

            it('should add field and update user info when nested field added', () => {
                const request = new AddFieldDto(field1.name, field1.partitioning, field1.properties);

                const newField = new NestedFieldDto(3, '3', createProperties('String'), 2);

                schemasService.setup(x => x.postField(app, schema.name, It.isAny(), 2, version))
                    .returns(() => of(versioned(newVersion, newField))).verifiable();

                schemasState.addField(schema, request, field2, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested).toEqual([nested1, nested2, newField]);
                expectToBeModified(schema_1);
            });

            it('should remove field and update user info when field removed', () => {
                schemasService.setup(x => x.deleteField(app, schema.name, field1.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.deleteField(schema, field1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields).toEqual([field2]);
                expectToBeModified(schema_1);
            });

            it('should remove field and update user info when nested field removed', () => {
                schemasService.setup(x => x.deleteField(app, schema.name, nested1.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.deleteField(schema, nested1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested).toEqual([nested2]);
                expectToBeModified(schema_1);
            });

            it('should sort fields and update user info when fields sorted', () => {
                schemasService.setup(x => x.putFieldOrdering(app, schema.name, [field2.fieldId, field1.fieldId], undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.sortFields(schema, [field2, field1], undefined, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields).toEqual([field2, field1]);
                expectToBeModified(schema_1);
            });

            it('should sort fields and update user info when nested fields sorted', () => {
                schemasService.setup(x => x.putFieldOrdering(app, schema.name, [nested2.fieldId, nested1.fieldId], 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.sortFields(schema, [nested2, nested1], field2, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested).toEqual([nested2, nested1]);
                expectToBeModified(schema_1);
            });

            it('should update field properties and update user info when field updated', () => {
                const request = { properties: createProperties('String') };

                schemasService.setup(x => x.putField(app, schema.name, field1.fieldId, request, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.updateField(schema, field1, request, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[0].properties).toBe(request.properties);
                expectToBeModified(schema_1);
            });

            it('should update field properties and update user info when nested field updated', () => {
                const request = { properties: createProperties('String') };

                schemasService.setup(x => x.putField(app, schema.name, nested1.fieldId, request, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.updateField(schema, nested1, request, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested[0].properties).toBe(request.properties);
                expectToBeModified(schema_1);
            });

            it('should mark field hidden and update user info when field hidden', () => {
                schemasService.setup(x => x.hideField(app, schema.name, field1.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.hideField(schema, field1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[0].isHidden).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should mark field hidden and update user info when nested field hidden', () => {
                schemasService.setup(x => x.hideField(app, schema.name, nested1.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.hideField(schema, nested1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested[0].isHidden).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should mark field disabled and update user info when field disabled', () => {
                schemasService.setup(x => x.disableField(app, schema.name, field1.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion)));

                schemasState.disableField(schema, field1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[0].isDisabled).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should mark field disabled and update user info when nested disabled', () => {
                schemasService.setup(x => x.disableField(app, schema.name, nested1.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.disableField(schema, nested1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested[0].isDisabled).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should mark field locked and update user info when field locked', () => {
                schemasService.setup(x => x.lockField(app, schema.name, field1.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.lockField(schema, field1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[0].isLocked).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should mark field locked and update user info when nested field locked', () => {
                schemasService.setup(x => x.lockField(app, schema.name, nested1.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.lockField(schema, nested1, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested[0].isLocked).toBeTruthy();
                expectToBeModified(schema_1);
            });

            it('should unmark field hidden and update user info when field shown', () => {
                schemasService.setup(x => x.showField(app, schema.name, field2.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.showField(schema, field2, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].isHidden).toBeFalsy();
                expectToBeModified(schema_1);
            });

            it('should unmark field hidden and update user info when nested field shown', () => {
                schemasService.setup(x => x.showField(app, schema.name, nested2.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.showField(schema, nested2, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].nested[1].isHidden).toBeFalsy();
                expectToBeModified(schema_1);
            });

            it('should unmark field disabled and update user info when field enabled', () => {
                schemasService.setup(x => x.enableField(app, schema.name, field2.fieldId, undefined, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.enableField(schema, field2, modified).subscribe();

                const schema_1 = <SchemaDetailsDto>schemasState.snapshot.schemas.at(1);

                expect(schema_1.fields[1].isDisabled).toBeFalsy();
                expectToBeModified(schema_1);
            });

            it('should unmark field disabled and update user info when nested field enabled', () => {
                schemasService.setup(x => x.enableField(app, schema.name, nested2.fieldId, 2, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

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
});