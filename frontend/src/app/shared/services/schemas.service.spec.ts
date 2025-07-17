/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AddFieldDto, ApiUrlConfig, ArrayFieldPropertiesDto, AssetsFieldPropertiesDto, BooleanFieldPropertiesDto, ChangeCategoryDto, ComponentFieldPropertiesDto, ComponentsFieldPropertiesDto, ConfigureFieldRulesDto, ConfigureUIFieldsDto, createProperties, CreateSchemaDto, DateTime, DateTimeFieldPropertiesDto, FieldDto, FieldRuleDto, GenerateSchemaDto, GenerateSchemaResponseDto, GeolocationFieldPropertiesDto, JsonFieldPropertiesDto, NestedFieldDto, NumberFieldPropertiesDto, ReferencesFieldPropertiesDto, Resource, ResourceLinkDto, SchemaDto, SchemaPropertiesDto, SchemaScriptsDto, SchemasDto, SchemasService, ScriptCompletions, StringFieldPropertiesDto, SynchronizeSchemaDto, TagsFieldPropertiesDto, UpdateFieldDto, UpdateSchemaDto, VersionTag } from '@app/shared/internal';

describe('SchemasService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        SchemasService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should throw if creating invalid property type', () => {
        const type: any = 'invalid';

        expect(() => createProperties(type)).toThrowError();
    });

    it('should make get request to get schemas',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let schemas: SchemasDto;
            schemasService.getSchemas('my-app').subscribe(result => {
                schemas = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                items: [
                    schemaResponse(12),
                    schemaResponse(13),
                ],
                _links: {
                    create: { method: 'POST', href: '/schemas' },
                },
            });

            expect(schemas!).toEqual(new SchemasDto({
                items: [
                    createSchema(12),
                    createSchema(13),
                ],
                _links: {
                    create: new ResourceLinkDto({ method: 'POST', href: '/schemas' }),
                },
            }));
        }));

    it('should make get request to get schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let schema: SchemaDto;
            schemasService.getSchema('my-app', 'my-schema').subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make post request to create schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new CreateSchemaDto({ name: 'name' });

            let schema: SchemaDto;
            schemasService.postSchema('my-app', dto).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make post request to generate schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new GenerateSchemaDto({ prompt: 'prompt', execute: true, numberOfContentItems: 1 });

            let schema: GenerateSchemaResponseDto;
            schemasService.generateSchema('my-app', dto).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/generate');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ log: [], schemaName: 'schema' });

            expect(schema!).toEqual(new GenerateSchemaResponseDto({ log: [], schemaName: 'schema' }));
        }));

    it('should make put request to update schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new UpdateSchemaDto({ label: 'label1' });

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema' },
                },
            };

            let schema: SchemaDto;
            schemasService.putSchema('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update schema scripts',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = {};

            const resource: Resource = {
                _links: {
                    'update/scripts': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/scripts' },
                },
            };

            let schema: SchemaDto;
            schemasService.putScripts('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/scripts');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update field rules',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new ConfigureFieldRulesDto({});

            const resource: Resource = {
                _links: {
                    'update/rules': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/rules' },
                },
            };

            let schema: SchemaDto;
            schemasService.putFieldRules('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/rules');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to synchronize schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new SynchronizeSchemaDto({});

            const resource: Resource = {
                _links: {
                    'update/sync': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/sync' },
                },
            };

            let schema: SchemaDto;
            schemasService.putSchemaSync('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/sync');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update category',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new ChangeCategoryDto({});

            const resource: Resource = {
                _links: {
                    'update/category': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/category' },
                },
            };

            let schema: SchemaDto;
            schemasService.putCategory('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/category');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update preview urls',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = {};

            const resource: Resource = {
                _links: {
                    'update/urls': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/preview-urls' },
                },
            };

            let schema: SchemaDto;
            schemasService.putPreviewUrls('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/preview-urls');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make post request to add field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new AddFieldDto({
                name: 'name',
                partitioning: 'invariant',
                properties: createProperties('Number'),
            });

            const resource: Resource = {
                _links: {
                    'fields/add': { method: 'POST', href: '/api/apps/my-app/schemas/my-schema/fields' },
                },
            };

            let schema: SchemaDto;
            schemasService.postField('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to publish schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    publish: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/publish' },
                },
            };

            let schema: SchemaDto;
            schemasService.publishSchema('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/publish');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to unpublish schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    unpublish: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/unpublish' },
                },
            };

            let schema: SchemaDto;
            schemasService.unpublishSchema('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/unpublish');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new UpdateFieldDto({ properties: createProperties('Number') });

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1' },
                },
            };

            let schema: SchemaDto;
            schemasService.putField('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update ui fields',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = new ConfigureUIFieldsDto({ fieldsInReferences: ['field1'] });

            const resource: Resource = {
                _links: {
                    'fields/ui': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/ui' },
                },
            };

            let schema: SchemaDto;
            schemasService.putUIFields('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/ui');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to update field ordering',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const dto = [1, 2, 3];

            const resource: Resource = {
                _links: {
                    'fields/order': { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/ordering' },
                },
            };

            let schema: SchemaDto;

            schemasService.putFieldOrdering('my-app', resource, dto, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/ordering');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to lock field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    lock: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1/lock' },
                },
            };

            let schema: SchemaDto;
            schemasService.lockField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/lock');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to enable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    enable: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1/enable' },
                },
            };

            let schema: SchemaDto;
            schemasService.enableField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/enable');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to disable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    disable: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1/disable' },
                },
            };

            let schema: SchemaDto;

            schemasService.disableField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/disable');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to show field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    show: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1/show' },
                },
            };

            let schema: SchemaDto;
            schemasService.showField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/show');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make put request to hide field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    hide: { method: 'PUT', href: '/api/apps/my-app/schemas/my-schema/fields/1/hide' },
                },
            };

            let schema: SchemaDto;
            schemasService.hideField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/hide');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make delete request to delete field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/schemas/my-schema/fields/1' },
                },
            };

            let schema: SchemaDto;
            schemasService.deleteField('my-app', resource, version).subscribe(result => {
                schema = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(schemaResponse(12));

            expect(schema!).toEqual(createSchema(12));
        }));

    it('should make delete request to delete schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/schemas/my-schema' },
                },
            };

            schemasService.deleteSchema('my-app', resource, version).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush({});
        }));

    it('should make get request to get content scripts completions',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let completions: ScriptCompletions;
            schemasService.getContentScriptsCompletion('my-app', 'my-schema').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/completion/content-scripts');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    it('should make get request to get content trigger completions',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let completions: ScriptCompletions;
            schemasService.getContentTriggerCompletion('my-app', 'my-schema').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/completion/content-triggers');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    it('should make get request to get field rules completions',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let completions: ScriptCompletions;
            schemasService.getFieldRulesCompletion('my-app', 'my-schema').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/completion/field-rules');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    it('should make get request to get preview urls completions',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {
            let completions: ScriptCompletions;
            schemasService.getPreviewUrlsCompletion('my-app', 'my-schema').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/completion/preview-urls');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    function schemaPropertiesResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            label: `label${key}`,
            contentsSidebarUrl: `url/to/contents/${key}`,
            contentSidebarUrl: `url/to/content/${key}`,
            contentEditorUrl: `url/to/editor/${key}`,
            contentsListUrl: `url/to/list/${key}`,
            tags: [
                `tags${key}`,
            ],
            validateOnPublish: id % 2 === 1,
            hints: `hints${key}`,
        };
    }

    function schemaResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            category: `schema-category${key}`,
            created: buildDate(id, 10),
            createdBy: `creator${id}`,
            isPublished: id % 3 === 0,
            isSingleton: false,
            lastModified: buildDate(id, 20),
            lastModifiedBy: `modifier${id}`,
            name: `schema-name${key}`,
            previewUrls: {
                Default: 'url',
            },
            properties: schemaPropertiesResponse(id, suffix),
            type: id % 2 === 0 ? 'Default' : 'Singleton',
            version: id,
            fields: [
                {
                    fieldId: 11,
                    name: 'field11',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Array',
                    },
                    nested: [
                        {
                            fieldId: 101,
                            name: 'field101',
                            isLocked: true,
                            isHidden: true,
                            isDisabled: true,
                            properties: {
                                fieldType: 'String',
                            },
                            _links: {},
                        },
                        {
                            fieldId: 102,
                            name: 'field102',
                            isLocked: true,
                            isHidden: true,
                            isDisabled: true,
                            properties: {
                                fieldType: 'Number',
                            },
                            _links: {},
                        },
                    ],
                    _links: {},
                },
                {
                    fieldId: 12,
                    name: 'field12',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Assets',
                    },
                    _links: {},
                },
                {
                    fieldId: 13,
                    name: 'field13',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Boolean',
                    },
                    _links: {},
                },
                {
                    fieldId: 14,
                    name: 'field14',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Component',
                    },
                    _links: {},
                },
                {
                    fieldId: 15,
                    name: 'field15',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Components',
                    },
                    _links: {},
                },
                {
                    fieldId: 16,
                    name: 'field16',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'DateTime',
                    },
                    _links: {},
                },
                {
                    fieldId: 17,
                    name: 'field17',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Geolocation',
                    },
                    _links: {},
                },
                {
                    fieldId: 18,
                    name: 'field18',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Json',
                    },
                    _links: {},
                },
                {
                    fieldId: 19,
                    name: 'field19',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Number',
                    },
                    _links: {},
                },
                {
                    fieldId: 20,
                    name: 'field20',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'References',
                    },
                    _links: {},
                },
                {
                    fieldId: 21,
                    name: 'field21',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'String',
                    },
                    _links: {},
                },
                {
                    fieldId: 22,
                    name: 'field22',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Tags',
                    },
                    _links: {},
                },
            ],
            fieldsInLists: ['field1'],
            fieldsInReferences: ['field1'],
            fieldRules: [
                { field: 'field1', action: 'Hide', condition: 'a === 2' },
            ],
            scripts: {
                query: '<script-query>',
                create: '<script-create>',
                change: '<script-change>',
                delete: '<script-delete>',
                update: '<script-update>',
            },
            _links: {
                update: { method: 'PUT', href: `/schemas/${id}` },
            },
        };
    }
});

function createSchemaProperties(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new SchemaPropertiesDto({
        label: `label${key}`,
        contentsSidebarUrl: `url/to/contents/${key}`,
        contentSidebarUrl: `url/to/content/${key}`,
        contentEditorUrl: `url/to/editor/${key}`,
        contentsListUrl: `url/to/list/${key}`,
        tags: [
            `tags${key}`,
        ],
        validateOnPublish: id % 2 === 1,
        hints: `hints${key}`,
    });
}

export function createSchema(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new SchemaDto({
        id: `id${id}`,
        category: `schema-category${key}`,
        created: DateTime.parseISO(buildDate(id, 10)),
        createdBy: `creator${id}`,
        isPublished: id % 3 === 0,
        isSingleton: false,
        lastModified: DateTime.parseISO(buildDate(id, 20)),
        lastModifiedBy: `modifier${id}`,
        name: `schema-name${key}`,
        previewUrls: {
            Default: 'url',
        },
        properties: createSchemaProperties(id, suffix),
        type: id % 2 === 0 ? 'Default' : 'Singleton',
        version: id + suffix.length,
        fields: [
            new FieldDto({
                fieldId: 11,
                name: 'field11',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new ArrayFieldPropertiesDto(),
                nested: [
                    new NestedFieldDto({
                        fieldId: 101,
                        name: 'field101',
                        isLocked: true,
                        isHidden: true,
                        isDisabled: true,
                        properties: new StringFieldPropertiesDto(),
                        _links: {},
                    }),
                    new NestedFieldDto({
                        fieldId: 102,
                        name: 'field102',
                        isLocked: true,
                        isHidden: true,
                        isDisabled: true,
                        properties: new NumberFieldPropertiesDto(),
                        _links: {},
                    }),
                ],
                _links: {},
            }),
            new FieldDto({
                fieldId: 12,
                name: 'field12',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new AssetsFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 13,
                name: 'field13',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new BooleanFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 14,
                name: 'field14',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new ComponentFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 15,
                name: 'field15',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new ComponentsFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 16,
                name: 'field16',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new DateTimeFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 17,
                name: 'field17',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new GeolocationFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 18,
                name: 'field18',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new JsonFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 19,
                name: 'field19',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new NumberFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 20,
                name: 'field20',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new ReferencesFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 21,
                name: 'field21',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new StringFieldPropertiesDto(),
                _links: {},
            }),
            new FieldDto({
                fieldId: 22,
                name: 'field22',
                isLocked: true,
                isHidden: true,
                isDisabled: true,
                partitioning: 'language',
                properties: new TagsFieldPropertiesDto(),
                _links: {},
            }),
        ],
        fieldsInLists: ['field1'],
        fieldsInReferences: ['field1'],
        fieldRules: [
            new FieldRuleDto({ field: 'field1', action: 'Hide', condition: 'a === 2' }),
        ],
        scripts: new SchemaScriptsDto({
            query: '<script-query>',
            create: '<script-create>',
            change: '<script-change>',
            delete: '<script-delete>',
            update: '<script-update>',
        }),
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/schemas/${id}` }),
        },
    });
}

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}
