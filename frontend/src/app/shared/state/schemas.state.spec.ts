/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of, onErrorResumeNextWith, throwError } from 'rxjs';
import { customMatchers } from 'src/spec/matchers';
import { IMock, It, Mock, Times } from 'typemoq';
import { AddFieldDto, ChangeCategoryDto, ConfigureUIFieldsDto, CreateSchemaDto, DialogService, SchemaDto, SchemasDto, SchemasService, SynchronizeSchemaDto, UpdateFieldDto, UpdateSchemaDto, versioned } from '@app/shared/internal';
import { createSchema } from '../services/schemas.service.spec';
import { TestValues } from './_test-helpers';
import { getCategoryTree, SchemasState } from './schemas.state';

describe('SchemasState', () => {
    const {
        app,
        appsState,
        newVersion,
    } = TestValues;

    const schema1 = createSchema(1);
    const schema2 = createSchema(2);

    const oldSchemas = new SchemasDto({
        items: [
            schema1,
            schema2,
        ],
        _links: {},
    });

    let dialogs: IMock<DialogService>;
    let schemasService: IMock<SchemasService>;
    let schemasState: SchemasState;

    beforeAll(function () {
        jasmine.addMatchers(customMatchers);
    });

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

            expect(schemasState.snapshot.schemas).toEqualIgnoringProps(oldSchemas.items);
            expect(schemasState.snapshot.isLoaded).toBeTruthy();

            schemasService.verifyAll();
        });

        it('should not remove custom category if loading schemas', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => of(oldSchemas)).verifiable();

            schemasState.addCategory('schema-category3');
            schemasState.load(true).subscribe();

            expect(schemasState.snapshot.isLoaded).toBeTruthy();
            expect(schemasState.snapshot.isLoading).toBeFalsy();
            expect(schemasState.snapshot.schemas).toEqualIgnoringProps(oldSchemas.items);

            schemasService.verifyAll();
        });

        it('should reset loading state if loading failed', () => {
            schemasService.setup(x => x.getSchemas(app))
                .returns(() => throwError(() => 'Service Error'));

            schemasState.load().pipe(onErrorResumeNextWith()).subscribe();

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

            expect([...schemasState.snapshot.addedCategories]).toEqualIgnoringProps(['schema-category3']);
        });

        it('should remove category', () => {
            schemasState.addCategory('schema-category3');
            schemasState.removeCategory('schema-category3');

            expect([...schemasState.snapshot.addedCategories]).toEqualIgnoringProps([]);
        });

        it('should return schema on select and reload if already loaded', () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema1)).verifiable(Times.exactly(2));

            schemasState.select(schema1.name).subscribe();
            schemasState.select(schema1.name).subscribe();

            expect().nothing();
        });

        it('should return schema on select and reload always', async () => {
            schemasService.setup(x => x.getSchema(app, schema1.name))
                .returns(() => of(schema1)).verifiable();

            const schemaSelected = await firstValueFrom(schemasState.select(schema1.name));

            expect(schemaSelected).toBe(schema1);
            expect(schemasState.snapshot.selectedSchema).toBe(schema1);
            expect(schemasState.snapshot.selectedSchema).toBe(<SchemaDto>schemasState.snapshot.schemas[0]);
        });

        it('should return null on select if unselecting schema', async () => {
            const schemaSelected = await firstValueFrom(schemasState.select(null));

            expect(schemaSelected).toBeNull();
            expect(schemasState.snapshot.selectedSchema).toBeNull();
        });

        it('should update schema if schema published', () => {
            const updated = createSchema(1, '_new');

            schemasService.setup(x => x.publishSchema(app, schema1, schema1.version))
                .returns(() => of(updated)).verifiable();

            schemasState.publish(schema1).subscribe();

            expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
        });

        it('should update schema if schema unpublished', () => {
            const updated = createSchema(1, '_new');

            schemasService.setup(x => x.unpublishSchema(app, schema1, schema1.version))
                .returns(() => of(updated)).verifiable();

            schemasState.unpublish(schema1).subscribe();
            expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
        });

        it('should update schema if schema category changed', () => {
            const updated = createSchema(1, '_new');

            const request = new ChangeCategoryDto({  name: 'my-new-category' });

            schemasService.setup(x => x.putCategory(app, schema1, It.isValue(request), schema1.version))
                .returns(() => of(updated)).verifiable();

            schemasState.changeCategory(schema1, request.name!).subscribe();

            expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
        });

        describe('with selection', () => {
            beforeEach(() => {
                schemasService.setup(x => x.getSchema(app, schema1.name))
                    .returns(() => of(schema1)).verifiable();

                schemasState.select(schema1.name).subscribe();
            });

            it('should update schema and selected schema if schema published', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.publishSchema(app, schema1, schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.publish(schema1).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if schema category changed', () => {
                const updated = createSchema(1, '_new');

                const request = new ChangeCategoryDto({  name: 'my-new-category' });

                schemasService.setup(x => x.putCategory(app, schema1, It.isValue(request), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.changeCategory(schema1, request.name!).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if schema updated', () => {
                const request = new UpdateSchemaDto({ label: 'name2_label', hints: 'name2_hints' });

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putSchema(app, schema1, It.isAny(), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.update(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if schema synced', () => {
                const request = new SynchronizeSchemaDto({});

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putSchemaSync(app, schema1, It.isAny(), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.synchronize(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if scripts configured', () => {
                const request = { query: '<query-script>' };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putScripts(app, schema1, It.isAny(), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureScripts(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if preview urls configured', () => {
                const request = { web: 'url' };

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putPreviewUrls(app, schema1, It.isAny(), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configurePreviewUrls(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should add schema to snapshot if created', () => {
                const request = new CreateSchemaDto({ name: 'newName' });

                const updated = createSchema(3, '_new');

                schemasService.setup(x => x.postSchema(app, request))
                    .returns(() => of(updated)).verifiable();

                schemasState.create(request).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(3);
                expect(schemasState.snapshot.schemas[2]).toEqualIgnoringProps(updated);
            });

            it('should remove schema from snapshot if deleted', () => {
                schemasService.setup(x => x.deleteSchema(app, schema1, schema1.version))
                    .returns(() => of(versioned(newVersion))).verifiable();

                schemasState.delete(schema1).subscribe();

                expect(schemasState.snapshot.schemas.length).toBe(1);
                expect(schemasState.snapshot.selectedSchema).toBeNull();
            });

            it('should update schema and selected schema if field added', async () => {
                const request = new AddFieldDto({ ...schema1.fields[0] });

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.postField(app, schema1, request, schema1.version))
                    .returns(() => of(updated)).verifiable();

                const schemaField = await firstValueFrom(schemasState.addField(schema1, request));

                expect(schemaField).toBeDefined();
                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if nested field added', async () => {
                const request = new AddFieldDto({ ...schema1.fields[0].nested![0] });

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.postField(app, schema1.fields[0], request, schema1.version))
                    .returns(() => of(updated)).verifiable();

                const schemaField = await firstValueFrom(schemasState.addField(schema1, request, schema1.fields[0]));

                expect(schemaField).toBeDefined();
                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field removed', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.deleteField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.deleteField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if ui fields configured', () => {
                const request = new ConfigureUIFieldsDto({ fieldsInLists: [schema1.fields[1].name] });

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putUIFields(app, schema1, request, schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.configureUIFields(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if fields sorted', () => {
                const request = [schema1.fields[1], schema1.fields[2]];

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema1, request.map(f => f.fieldId), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if nested fields sorted', () => {
                const request = [schema1.fields[1], schema1.fields[2]];

                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.putFieldOrdering(app, schema1.fields[0], request.map(f => f.fieldId), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.orderFields(schema1, request, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field updated', () => {
                const updated = createSchema(1, '_new');

                const request = new UpdateFieldDto({ ...schema1.fields[0] });

                schemasService.setup(x => x.putField(app, schema1.fields[0], request, schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.updateField(schema1, schema1.fields[0], request).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field hidden', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.hideField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.hideField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field disabled', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.disableField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated));

                schemasState.disableField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field locked', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.lockField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.lockField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field shown', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.showField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.showField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema and selected schema if field enabled', () => {
                const updated = createSchema(1, '_new');

                schemasService.setup(x => x.enableField(app, schema1.fields[0], schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.enableField(schema1, schema1.fields[0]).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });

            it('should update schema with matching category', () => {
                const updated = createSchema(1, '_new');

                const request = new ChangeCategoryDto({ name: 'new-name' });

                schemasService.setup(x => x.putCategory(app, schema1, It.isValue(request), schema1.version))
                    .returns(() => of(updated)).verifiable();

                schemasState.renameCategory('schema-category1', request.name!).subscribe();

                expect(schemasState.snapshot.schemas).toEqualIgnoringProps([updated, schema2]);
                expect(schemasState.snapshot.selectedSchema).toEqualIgnoringProps(updated);
            });
        });
    });

    describe('Categories', () => {
        it('should be build from schemas with undefined categories', () => {
            const schemaDefault = createSchema(6);
            const schemaComponent = createSchema(7);

            (schemaDefault as any)['category'] = '';
            (schemaComponent as any)['category'] = '';
            (schemaComponent as any)['type'] = 'Component';

            const result = getCategoryTree([schemaDefault, schemaComponent], new Set<string>());

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [schemaComponent],
                    schemasFiltered: [schemaComponent],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
                {
                    displayName: 'i18n:common.schemas',
                    schemas: [schemaDefault],
                    schemasFiltered: [schemaDefault],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
            ]);
        });

        it('should be build from schemas with defined categories', () => {
            const result = getCategoryTree([schema1, schema2], new Set<string>());

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'i18n:common.schemas',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'schema-category1',
                    name: 'schema-category1',
                    schemas: [schema1],
                    schemasFiltered: [schema1],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
                {
                    displayName: 'schema-category2',
                    name: 'schema-category2',
                    schemas: [schema2],
                    schemasFiltered: [schema2],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
            ]);
        });

        it('should be build from schemas and custom name', () => {
            const result = getCategoryTree([schema1, schema2], new Set<string>(['schema-category3']));

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'i18n:common.schemas',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'schema-category1',
                    name: 'schema-category1',
                    schemas: [schema1],
                    schemasFiltered: [schema1],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
                {
                    displayName: 'schema-category2',
                    name: 'schema-category2',
                    schemas: [schema2],
                    schemasFiltered: [schema2],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
                {
                    displayName: 'schema-category3',
                    name: 'schema-category3',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
            ]);
        });

        it('should be build from schemas and filter', () => {
            const result = getCategoryTree([schema1, schema2], new Set<string>(), '1');

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'i18n:common.schemas',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
                {
                    displayName: 'schema-category1',
                    name: 'schema-category1',
                    schemas: [schema1],
                    schemasFiltered: [schema1],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
                {
                    displayName: 'schema-category2',
                    name: 'schema-category2',
                    schemas: [schema2],
                    schemasFiltered: [],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                },
            ]);
        });

        it('should be build from schemas with nested categories', () => {
            const schemaA = createSchema(3);
            const schemaAB = createSchema(4);

            (schemaA as any)['category'] = 'A';
            (schemaAB as any)['category'] = 'A/B';

            const result = getCategoryTree([schema1, schema2, schemaA, schemaAB], new Set<string>());

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                }, {
                    displayName: 'i18n:common.schemas',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                }, {
                    displayName: 'A',
                    name: 'A',
                    schemas: [schemaA],
                    schemasFiltered: [schemaA],
                    countSchemasInSubtree: 2,
                    countSchemasInSubtreeFiltered: 2,
                    categories: [{
                        displayName: 'B',
                        name: 'A/B',
                        schemas: [schemaAB],
                        schemasFiltered: [schemaAB],
                        countSchemasInSubtree: 1,
                        countSchemasInSubtreeFiltered: 1,
                        categories: [],
                    }],
                }, {
                    displayName: 'schema-category1',
                    name: 'schema-category1',
                    schemas: [schema1],
                    schemasFiltered: [schema1],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                }, {
                    displayName: 'schema-category2',
                    name: 'schema-category2',
                    schemas: [schema2],
                    schemasFiltered: [schema2],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
            ]);
        });

        it('should be build from schemas and custom name with nested categories', () => {
            const result = getCategoryTree([schema1, schema2], new Set<string>(['A/B']));

            expect(result).toEqualIgnoringProps([
                {
                    displayName: 'i18n:common.components',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                }, {
                    displayName: 'i18n:common.schemas',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [],
                }, {
                    displayName: 'A',
                    name: 'A',
                    schemas: [],
                    schemasFiltered: [],
                    countSchemasInSubtree: 0,
                    countSchemasInSubtreeFiltered: 0,
                    categories: [{
                        displayName: 'B',
                        name: 'A/B',
                        schemas: [],
                        schemasFiltered: [],
                        countSchemasInSubtree: 0,
                        countSchemasInSubtreeFiltered: 0,
                        categories: [],
                    }],
                }, {
                    displayName: 'schema-category1',
                    name: 'schema-category1',
                    schemas: [schema1],
                    schemasFiltered: [schema1],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                }, {
                    displayName: 'schema-category2',
                    name: 'schema-category2',
                    schemas: [schema2],
                    schemasFiltered: [schema2],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                },
            ]);
        });
    });

    it('should be build from schemas with nested categories and filter', () => {
        const schemaA = createSchema(3);
        const schemaAB = createSchema(4);

        (schemaA as any)['category'] = 'A';
        (schemaAB as any)['category'] = 'A/B';

        const result = getCategoryTree([schema1, schema2, schemaA, schemaAB], new Set<string>(), '4');

        expect(result).toEqualIgnoringProps([
            {
                displayName: 'i18n:common.components',
                schemas: [],
                schemasFiltered: [],
                countSchemasInSubtree: 0,
                countSchemasInSubtreeFiltered: 0,
                categories: [],
            }, {
                displayName: 'i18n:common.schemas',
                schemas: [],
                schemasFiltered: [],
                countSchemasInSubtree: 0,
                countSchemasInSubtreeFiltered: 0,
                categories: [],
            },
            {
                displayName: 'A',
                name: 'A',
                schemas: [schemaA],
                schemasFiltered: [],
                countSchemasInSubtree: 2,
                countSchemasInSubtreeFiltered: 1,
                categories: [{
                    displayName: 'B',
                    name: 'A/B',
                    schemas: [schemaAB],
                    schemasFiltered: [schemaAB],
                    countSchemasInSubtree: 1,
                    countSchemasInSubtreeFiltered: 1,
                    categories: [],
                }],
            }, {
                displayName: 'schema-category1',
                name: 'schema-category1',
                schemas: [schema1],
                schemasFiltered: [],
                countSchemasInSubtree: 1,
                countSchemasInSubtreeFiltered: 0,
                categories: [],
            }, {
                displayName: 'schema-category2',
                name: 'schema-category2',
                schemas: [schema2],
                schemasFiltered: [],
                countSchemasInSubtree: 1,
                countSchemasInSubtreeFiltered: 0,
                categories: [],
            },
        ]);
    });
});
