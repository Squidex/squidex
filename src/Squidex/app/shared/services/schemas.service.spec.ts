/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AddFieldDto,
    AnalyticsService,
    ApiUrlConfig,
    createProperties,
    CreateSchemaDto,
    DateTime,
    FieldDto,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaCategoryDto,
    UpdateSchemaDto,
    UpdateSchemaScriptsDto,
    Version
} from './../';

describe('SchemasService', () => {
    const now = DateTime.now();
    const user = 'me';
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                SchemasService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should throw if creating invalid property type', () => {
        expect(() => createProperties('invalid')).toThrow('Invalid properties type');
    });

    it('should make get request to get schemas',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        let schemas: SchemaDto[];

        schemasService.getSchemas('my-app').subscribe(result => {
            schemas = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: 'id1',
                name: 'name1',
                category: 'category1',
                properties: {
                    label: 'label1',
                    hints: 'hints1'
                },
                isPublished: true,
                created: '2016-12-12T10:10',
                createdBy: 'Created1',
                lastModified: '2017-12-12T10:10',
                lastModifiedBy: 'LastModifiedBy1',
                version: 11,
                data: {}
            },
            {
                id: 'id2',
                name: 'name2',
                category: 'category2',
                properties: {
                    label: 'label2',
                    hints: 'hints2'
                },
                isPublished: true,
                created: '2016-10-12T10:10',
                createdBy: 'Created2',
                lastModified: '2017-10-12T10:10',
                lastModifiedBy: 'LastModifiedBy2',
                version: 22,
                data: {}
            }
        ]);

        expect(schemas!).toEqual(
            [
                new SchemaDto('id1', 'name1', 'category1', new SchemaPropertiesDto('label1', 'hints1'), true,
                    DateTime.parseISO_UTC('2016-12-12T10:10'), 'Created1',
                    DateTime.parseISO_UTC('2017-12-12T10:10'), 'LastModifiedBy1',
                    new Version('11')),
                new SchemaDto('id2', 'name2', 'category2', new SchemaPropertiesDto('label2', 'hints2'), true,
                    DateTime.parseISO_UTC('2016-10-12T10:10'), 'Created2',
                    DateTime.parseISO_UTC('2017-10-12T10:10'), 'LastModifiedBy2',
                    new Version('22'))
            ]);
    }));

    it('should make get request to get schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        let schema: SchemaDetailsDto;

        schemasService.getSchema('my-app', 'my-schema').subscribe(result => {
            schema = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            name: 'name1',
            category: 'category1',
            isPublished: true,
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            properties: {
                label: 'label1',
                hints: 'hints1'
            },
            fields: [
                {
                    fieldId: 11,
                    name: 'field11',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Array'
                    },
                    nested: [
                        {
                            fieldId: 101,
                            name: 'field101',
                            isLocked: true,
                            isHidden: true,
                            isDisabled: true,
                            properties: {
                                fieldType: 'String'
                            }
                        },
                        {
                            fieldId: 102,
                            name: 'field102',
                            isLocked: true,
                            isHidden: true,
                            isDisabled: true,
                            properties: {
                                fieldType: 'Number'
                            }
                        }
                    ]
                },
                {
                    fieldId: 12,
                    name: 'field12',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Assets'
                    }
                },
                {
                    fieldId: 13,
                    name: 'field13',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Boolean'
                    }
                },
                {
                    fieldId: 14,
                    name: 'field14',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'DateTime'
                    }
                },
                {
                    fieldId: 15,
                    name: 'field15',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Geolocation'
                    }
                },
                {
                    fieldId: 16,
                    name: 'field16',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Json'
                    }
                },
                {
                    fieldId: 17,
                    name: 'field17',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Number'
                    }
                },
                {
                    fieldId: 18,
                    name: 'field18',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'References'
                    }
                },
                {
                    fieldId: 19,
                    name: 'field19',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'String'
                    }
                },
                {
                    fieldId: 20,
                    name: 'field20',
                    isLocked: true,
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Tags'
                    }
                }
            ],
            scriptQuery: '<script-query>',
            scriptCreate: '<script-create>',
            scriptUpdate: '<script-update>',
            scriptDelete: '<script-delete>',
            scriptChange: '<script-change>'
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(schema!).toEqual(
            new SchemaDetailsDto('id1', 'name1', 'category1', new SchemaPropertiesDto('label1', 'hints1'), true,
                DateTime.parseISO_UTC('2016-12-12T10:10'), 'Created1',
                DateTime.parseISO_UTC('2017-12-12T10:10'), 'LastModifiedBy1',
                new Version('2'),
                [
                    new RootFieldDto(11, 'field11', createProperties('Array'), 'language', true, true, true, [
                        new NestedFieldDto(101, 'field101', createProperties('String'), 11, true, true, true),
                        new NestedFieldDto(102, 'field102', createProperties('Number'), 11, true, true, true)
                    ]),
                    new RootFieldDto(12, 'field12', createProperties('Assets'), 'language', true, true, true),
                    new RootFieldDto(13, 'field13', createProperties('Boolean'), 'language', true, true, true),
                    new RootFieldDto(14, 'field14', createProperties('DateTime'), 'language', true, true, true),
                    new RootFieldDto(15, 'field15', createProperties('Geolocation'), 'language', true, true, true),
                    new RootFieldDto(16, 'field16', createProperties('Json'), 'language', true, true, true),
                    new RootFieldDto(17, 'field17', createProperties('Number'), 'language', true, true, true),
                    new RootFieldDto(18, 'field18', createProperties('References'), 'language', true, true, true),
                    new RootFieldDto(19, 'field19', createProperties('String'), 'language', true, true, true),
                    new RootFieldDto(20, 'field20', createProperties('Tags'), 'language', true, true, true)
                ],
                '<script-query>',
                '<script-create>',
                '<script-update>',
                '<script-delete>',
                '<script-change>'));
    }));

    it('should make post request to create schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new CreateSchemaDto('name');

        let schema: SchemaDetailsDto;

        schemasService.postSchema('my-app', dto, user, now).subscribe(result => {
            schema = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: '1'
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(schema!).toEqual(
            new SchemaDetailsDto('1', dto.name, '', new SchemaPropertiesDto(), false, now, user, now, user, new Version('2'), []));
    }));

    it('should make put request to update schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateSchemaDto('label', 'hints');

        schemasService.putSchema('my-app', 'my-schema', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update schema scripts',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateSchemaScriptsDto();

        schemasService.putScripts('my-app', 'my-schema', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/scripts');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update category',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateSchemaCategoryDto();

        schemasService.putCategory('my-app', 'my-schema', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/category');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make post request to add field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new AddFieldDto('name', 'invariant', createProperties('Number'));

        let field: FieldDto;

        schemasService.postField('my-app', 'my-schema', dto, undefined, version).subscribe(result => {
            field = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 123 });

        expect(field!).toEqual(new RootFieldDto(123, dto.name, dto.properties, dto.partitioning));
    }));

    it('should make put request to publish schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.publishSchema('my-app', 'my-schema', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/publish');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to unpublish schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.unpublishSchema('my-app', 'my-schema', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/unpublish');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make post request to add nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new AddFieldDto('name', 'invariant', createProperties('Number'));

        let field: FieldDto;

        schemasService.postField('my-app', 'my-schema', dto, 13, version).subscribe(result => {
            field = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 123 });

        expect(field!).toEqual(new NestedFieldDto(123, dto.name, dto.properties, 13));
    }));

    it('should make put request to update field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateFieldDto(createProperties('Number'));

        schemasService.putField('my-app', 'my-schema', 1, dto, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateFieldDto(createProperties('Number'));

        schemasService.putField('my-app', 'my-schema', 1, dto, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update field ordering',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = [1, 2, 3];

        schemasService.putFieldOrdering('my-app', 'my-schema', dto, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/ordering');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update nested field ordering',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = [1, 2, 3];

        schemasService.putFieldOrdering('my-app', 'my-schema', dto, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/ordering');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to lock field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.lockField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/lock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to lock nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.lockField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1/lock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to enable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.enableField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/enable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to enable nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.enableField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1/enable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to disable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.disableField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/disable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to disable nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.disableField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1/disable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to show field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.showField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/show');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to show nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.showField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1/show');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to hide field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.hideField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/hide');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to hide nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.hideField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1/hide');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make delete request to delete field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.deleteField('my-app', 'my-schema', 1, undefined, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make delete request to delete nested field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.deleteField('my-app', 'my-schema', 1, 13, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/13/nested/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make delete request to delete schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.deleteSchema('my-app', 'my-schema', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));
});