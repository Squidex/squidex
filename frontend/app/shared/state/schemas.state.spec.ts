/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, FieldDto, SchemaDetailsDto, SchemasService, UpdateSchemaCategoryDto, versioned } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { createSchema, createSchemaDetails } from './../services/schemas.service.spec';
import { SchemaCategory, SchemasState } from './schemas.state';
import { TestValues } from './_test-helpers';

describe('SchemasState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const schema1 = createSchema(1);
    const schema2 = createSchema(2);

    const oldSchemas = {
        canCreate: true,
        items: [
            schema1,
            schema2
        ],
        _links: {}
    };

    const schema = createSchemaDetails(1);

    let dialogs: IMock<DialogService>;
    let schemasService: IMock<SchemasService>;
    let schemasState: SchemasState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        schemasService = Mock.ofType<SchemasService>();
        schemasState = new SchemasState(appsState.object, dialogs.object, schemasService.object);
    });

    afterEach(() => {
        schemasService.verifyAll();
    });

    describe('Loading', () => {
        it('should load schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load().subscribe();

            expect(schemasState.snapshot.schemas).toEqual(oldSchemas.items);
            expect(schemasState.snapshot.isLoaded).toBeTruthy();

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { name: '', upper: '', schemas: [] },
                { name: 'category1', upper: 'CATEGORY1', schemas: [schema1] },
                { name: 'category2', upper: 'CATEGORY2', schemas: [schema2] }
            ]);

            schemasService.verifyAll();
        });

        it('should not remove custom category when loading schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.addCategory('category3');
            schemasState.load(true).subscribe();

            expect(schemasState.snapshot.isLoaded).toBeTruthy();
            expect(schemasState.snapshot.isLoading).toBeFalsy();
            expect(schemasState.snapshot.schemas).toEqual(oldSchemas.items);

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { name: '', upper: '', schemas: [] },
                { name: 'category1', upper: 'CATEGORY1', schemas: [schema1] },
                { name: 'category2', upper: 'CATEGORY2', schemas: [schema2] },
                { name: 'category3', upper: 'CATEGORY3', schemas: [] }
            ]);

            schemasService.verifyAll();
        });

        it('should reset loading when loading failed', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => throwError('error'));

            schemasState.load().pipe(onErrorResumeNext()).subscribe();

            expect(schemasState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load when reload is true', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should not load if already loaded', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load(true).subscribe();
            schemasState.loadIfNotLoaded().subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load if not loaded yet', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.load(true).subscribe();
            schemasState.loadIfNotLoaded().subscribe();

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

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { name: '', upper: '', schemas: [] },
                { name: 'category1', upper: 'CATEGORY1', schemas: [schema1] },
                { name: 'category2', upper: 'CATEGORY2', schemas: [schema2] },
                { name: 'category3', upper: 'CATEGORY3', schemas: [] }
            ]);
        });

        it('should not remove category with schemas', () => {
            schemasState.addCategory('category1');

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { name: '', upper: '', schemas: [] },
                { name: 'category1', upper: 'CATEGORY1', schemas: [schema1] },
                { name: 'category2', upper: 'CATEGORY2', schemas: [schema2] }
            ]);
        });

        it('should remove category', () => {
            schemasState.addCategory('category3');
            schemasState.removeCategory('category3');

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { name: '', upper: '', schemas: [] },
                { name: 'category1', upper: 'CATEGORY1', schemas: [schema1] },
                { name: 'category2', upper: 'CATEGORY2', schemas: [schema2] }
            ]);
        });

        it('should return schema on select and reload when already loaded', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema)).verifiable(Times.exactly(2));

            schemasState.select(schema1.name).subscribe();
            schemasState.select(schema1.name).subscribe();

            expect().nothing();
        });

        it('should return schema on select and reload always', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema)).verifiable();

            let selectedSchema: SchemaDetailsDto;

            schemasState.select(schema1.name).subscribe(x => {
                selectedSchema = x!;
            });

            expect(selectedSchema!).toBe(schema);
            expect(schemasState.snapshot.selectedSchema).toBe(schema);
            expect(schemasState.snapshot.selectedSchema).toBe(<SchemaDetailsDto>schemasState.snapshot.schemas[0]);
        });

        it('should return schema on get and cache it', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema)).verifiable(Times.once());

            schemasState.loadSchema(schema1.name, true).subscribe();
            schemasState.loadSchema(schema1.name, true).subscribe();

            expect().nothing();
        });

        it('should return schema on get and reuse it from select when caching', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema)).verifiable(Times.once());

            schemasState.select(schema1.name).subscribe();
            schemasState.loadSchema(schema1.name, true).subscribe();

            expect().nothing();
        });

        it('should return null on select when loading failed', () => {
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

        it('should update schema when schema published', () => {
            const updated = createSchemaDetails(1, '_new');

            schemasService.setup(x => x.publishSchema(app, schema1, version))
                .returns(() => of(updated)).verifiable();

            schemasState.publish(schema1).subscribe();

            const schema1New = schemasState.snapshot.schemas[0];

            expect(schema1New).toEqual(updated);
        });

        it('should update schema when schema unpublished', () => {
            const updated = createSchemaDetails(1, '_new');

            schemasService.setup(x => x.unpublishSchema(app, schema1, version))
                .returns(() => of(updated)).verifiable();

            schemasState.unpublish(schema1).subscribe();

            const schema1New = schemasState.snapshot.schemas[0];

            expect(schema1New).toEqual(updated);
        });

        it('should update schema when schema category changed', () => {
            const category = 'my-new-category';

            const updated = createSchemaDetails(1, '_new');

            schemasService.setup(x => x.putCategory(app, schema1, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                .returns(() => of(updated)).verifiable();

            schemasState.changeCategory(schema1, category).subscribe();

            const schema1New = schemasState.snapshot.schemas[0];

            expect(schema1New).toEqual(updated);
        });

        describe('with selection', () => {
            beforeEach(() => {
                schemasService.setup(x => x.getSchema(app, schema1.name))
                    .returns(() => of(schema)).verifiable();

                schemasState.select(schema1.name).subscribe();
            });

            it('should update schema and selected schema when schema published', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.publishSchema(app, schema1, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.publish(schema1).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when schema category changed', () => {
                const category = 'my-new-category';

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putCategory(app, schema1, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.changeCategory(schema1, category).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when schema updated', () => {
                const request = { label: 'name2_label', hints: 'name2_hints' };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putSchema(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.update(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when schema synced', () => {
                const request = {};

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putSchemaSync(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.synchronize(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when scripts configured', () => {
                const request = { query: '<query-script>' };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putScripts(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureScripts(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when preview urls configured', () => {
                const request = { web: 'url' };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putPreviewUrls(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configurePreviewUrls(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should add schema to snapshot when created', () => {
                const request = { name: 'newName' };

                const updated = createSchemaDetails(3, '_new');

                schemasService.setup(x => x.postSchema(app, request))
                    .returns(() => of(updated)).verifiable();

                schemasState.create(request).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(3);
                expect(schemasState.snapshot.schemas[2]).toEqual(updated);
            });

            it('should remove schema from snapshot when deleted', () => {
                schemasService.setup(x => x.deleteSchema(app, schema1, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.delete(schema1).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(1);
                expect(schemasState.snapshot.selectedSchema).toBeNull();
            });

            it('should update schema and selected schema when field added', () => {
                const request = { ...schema.fields[0] };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.postField(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                let newField: FieldDto;

                schemasState.addField(schema1, request).subscribe(result => {
                    newField = result;
                });

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
                expect(newField!).toBeDefined();
            });

            it('should update schema and selected schema when nested field added', () => {
                const request = { ...schema.fields[0].nested[0] };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.postField(app, schema.fields[0], It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                let newField: FieldDto;

                schemasState.addField(schema1, request, schema.fields[0]).subscribe(result => {
                    newField = result;
                });

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
                expect(newField!).toBeDefined();
            });

            it('should update schema and selected schema when field removed', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.deleteField(app, schema.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.deleteField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when ui fields configured', () => {
                const request = { fieldsInLists: [schema.fields[1].name] };

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putUIFields(app, schema1, request, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureUIFields(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when fields sorted', () => {
                const request = [schema.fields[1], schema.fields[2]];

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema1, request.map(f => f.fieldId), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when nested fields sorted', () => {
                const request = [schema.fields[1], schema.fields[2]];

                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema.fields[0], request.map(f => f.fieldId), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field updated', () => {
                const updated = createSchemaDetails(1, '_new');

                const request = { ...schema.fields[0] };

                schemasService.setup(x => x.putField(app, schema.fields[0], request, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.updateField(schema1, schema.fields[0], request).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field hidden', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.hideField(app, schema.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.hideField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field disabled', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.disableField(app, schema.fields[0], version))
                    .returns(() => of(updated));

                schemasState.disableField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field locked', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.lockField(app, schema.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.lockField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field shown', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.showField(app, schema.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.showField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema when field enabled', () => {
                const updated = createSchemaDetails(1, '_new');

                schemasService.setup(x => x.enableField(app, schema.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.enableField(schema1, schema.fields[0]).subscribe();

                const schema1New = <SchemaDetailsDto>schemasState.snapshot.schemas[0];

                expect(schema1New).toEqual(updated);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });
        });
    });
});

function getCategories(schemasState: SchemasState) {
    let categories: ReadonlyArray<SchemaCategory>;

    schemasState.categories.subscribe(result => {
        categories = result;
    });

    return categories!;
}
