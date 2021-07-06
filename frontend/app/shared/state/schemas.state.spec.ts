/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, FieldDto, SchemaDto, SchemasService, UpdateSchemaCategoryDto, versioned } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { createSchema } from './../services/schemas.service.spec';
import { SchemaCategory, SchemasState } from './schemas.state';
import { TestValues } from './_test-helpers';

describe('SchemasState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

    const schema1 = createSchema(1);
    const schema2 = createSchema(2);

    const oldSchemas = {
        canCreate: true,
        items: [
            schema1,
            schema2,
        ],
        _links: {},
    };

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
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
            ]);

            schemasService.verifyAll();
        });

        it('should not remove custom category if loading schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.addCategory('schema-category3');
            schemasState.load(true).subscribe();

            expect(schemasState.snapshot.isLoaded).toBeTruthy();
            expect(schemasState.snapshot.isLoading).toBeFalsy();
            expect(schemasState.snapshot.schemas).toEqual(oldSchemas.items);

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
                { displayName: 'schema-category3', name: 'schema-category3', schemas: [] },
            ]);

            schemasService.verifyAll();
        });

        it('should reset loading state if loading failed', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => throwError(() => 'Service Error'));

            schemasState.load().pipe(onErrorResumeNext()).subscribe();

            expect(schemasState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
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
            schemasState.addCategory('schema-category3');

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
                { displayName: 'schema-category3', name: 'schema-category3', schemas: [] },
            ]);
        });

        it('should not remove category with schemas', () => {
            schemasState.removeCategory('schema-category1');

            const categories = getCategories(schemasState);

            expect(categories!).toEqual([
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
            ]);
        });

        it('should remove category', () => {
            schemasState.addCategory('schema-category3');

            const categories1 = getCategories(schemasState);

            expect(categories1).toEqual([
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
                { displayName: 'schema-category3', name: 'schema-category3', schemas: [] },
            ]);

            schemasState.removeCategory('schema-category3');

            const categories2 = getCategories(schemasState);

            expect(categories2).toEqual([
                { displayName: 'i18n:common.components', schemas: [] },
                { displayName: 'i18n:common.schemas', schemas: [] },
                { displayName: 'schema-category1', name: 'schema-category1', schemas: [schema1] },
                { displayName: 'schema-category2', name: 'schema-category2', schemas: [schema2] },
            ]);
        });

        it('should return schema on select and reload if already loaded', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema1)).verifiable(Times.exactly(2));

            schemasState.select(schema1.name).subscribe();
            schemasState.select(schema1.name).subscribe();

            expect().nothing();
        });

        it('should return schema on select and reload always', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema1)).verifiable();

            let schemaSelected: SchemaDto;

            schemasState.select(schema1.name).subscribe(x => {
                schemaSelected = x!;
            });

            expect(schemaSelected!).toBe(schema1);
            expect(schemasState.snapshot.selectedSchema).toBe(schema1);
            expect(schemasState.snapshot.selectedSchema).toBe(<SchemaDto>schemasState.snapshot.schemas[0]);
        });

        it('should return null on select if unselecting schema', () => {
            let schemaSelected: SchemaDto;

            schemasState.select(null).subscribe(x => {
                schemaSelected = x!;
            });

            expect(schemaSelected!).toBeNull();
            expect(schemasState.snapshot.selectedSchema).toBeNull();
        });

        it('should update schema if schema published', () => {
            const updated = createSchema(1, '_new');

            schemasService.setup(x => x.publishSchema(app, schema1, version))
                .returns(() => of(updated)).verifiable();

            schemasState.publish(schema1).subscribe();

            expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
        });

        it('should update schema if schema unpublished', () => {
            const updated = createSchema(1, '_new');

            schemasService.setup(x => x.unpublishSchema(app, schema1, version))
                .returns(() => of(updated)).verifiable();

            schemasState.unpublish(schema1).subscribe();
            expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
        });

        it('should update schema if schema category changed', () => {
            const category = 'my-new-category';

            const updated = createSchema(1, '_new');

            schemasService.setup(x => x.putCategory(app, schema1, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                .returns(() => of(updated)).verifiable();

            schemasState.changeCategory(schema1, category).subscribe();

            expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
        });

        describe('with selection', () => {
            beforeEach(() => {
                schemasService.setup(x => x.getSchema(app, schema1.name))
                    .returns(() => of(schema1)).verifiable();

                schemasState.select(schema1.name).subscribe();
            });

            it('should update schema and selected schema if schema published', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.publishSchema(app, schema1, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.publish(schema1).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if schema category changed', () => {
                const category = 'my-new-category';

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putCategory(app, schema1, It.is<UpdateSchemaCategoryDto>(i => i.name === category), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.changeCategory(schema1, category).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if schema updated', () => {
                const request = { label: 'name2_label', hints: 'name2_hints' };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putSchema(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.update(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if schema synced', () => {
                const request = {};

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putSchemaSync(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.synchronize(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if scripts configured', () => {
                const request = { query: '<query-script>' };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putScripts(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureScripts(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if preview urls configured', () => {
                const request = { web: 'url' };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putPreviewUrls(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configurePreviewUrls(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should add schema to snapshot if created', () => {
                const request = { name: 'newName' };

                const updated = createSchema(3, '_new');

                schemasService.setup(x => x.postSchema(app, request))
                    .returns(() => of(updated)).verifiable();

                schemasState.create(request).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(3);
                expect(schemasState.snapshot.schemas[2]).toEqual(updated);
            });

            it('should remove schema from snapshot if deleted', () => {
                schemasService.setup(x => x.deleteSchema(app, schema1, version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.delete(schema1).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(1);
                expect(schemasState.snapshot.selectedSchema).toBeNull();
            });

            it('should update schema and selected schema if field added', () => {
                const request = { ...schema1.fields[0] };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.postField(app, schema1, It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                let newField: FieldDto;

                schemasState.addField(schema1, request).subscribe(result => {
                    newField = result;
                });

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
                expect(newField!).toBeDefined();
            });

            it('should update schema and selected schema if nested field added', () => {
                const request = { ...schema1.fields[0].nested[0] };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.postField(app, schema1.fields[0], It.isAny(), version))
                    .returns(() => of(updated)).verifiable();

                let newField: FieldDto;

                schemasState.addField(schema1, request, schema1.fields[0]).subscribe(result => {
                    newField = result;
                });

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
                expect(newField!).toBeDefined();
            });

            it('should update schema and selected schema if field removed', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.deleteField(app, schema1.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.deleteField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if ui fields configured', () => {
                const request = { fieldsInLists: [schema1.fields[1].name] };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putUIFields(app, schema1, request, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureUIFields(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if fields sorted', () => {
                const request = [schema1.fields[1], schema1.fields[2]];

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema1, request.map(f => f.fieldId), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if nested fields sorted', () => {
                const request = [schema1.fields[1], schema1.fields[2]];

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema1.fields[0], request.map(f => f.fieldId), version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field updated', () => {
                const updated = createSchema(1, '_new');

                const request = { ...schema1.fields[0] };

                schemasService.setup(x => x.putField(app, schema1.fields[0], request, version))
                    .returns(() => of(updated)).verifiable();

                schemasState.updateField(schema1, schema1.fields[0], request).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field hidden', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.hideField(app, schema1.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.hideField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field disabled', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.disableField(app, schema1.fields[0], version))
                    .returns(() => of(updated));

                schemasState.disableField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field locked', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.lockField(app, schema1.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.lockField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field shown', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.showField(app, schema1.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.showField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqual(updated);
            });

            it('should update schema and selected schema if field enabled', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.enableField(app, schema1.fields[0], version))
                    .returns(() => of(updated)).verifiable();

                schemasState.enableField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqual([updated, schema2]);
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
