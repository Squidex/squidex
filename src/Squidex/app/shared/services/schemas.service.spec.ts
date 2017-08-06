/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AddFieldDto,
    ApiUrlConfig,
    CreateSchemaDto,
    createProperties,
    DateTime,
    FieldDto,
    LocalCacheService,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaDto,
    Version
} from './../';

describe('SchemaDto', () => {
    const properties = new SchemaPropertiesDto('Name', null);

    it('should update isPublished property and user info when publishing', () => {
        const now = DateTime.now();

        const schema_1 = new SchemaDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null);
        const schema_2 = schema_1.publish('me', now);

        expect(schema_2.isPublished).toBeTruthy();
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update isPublished property and user info when unpublishing', () => {
        const now = DateTime.now();

        const schema_1 = new SchemaDto('1', 'name', properties, true, 'other', 'other', DateTime.now(), DateTime.now(), null);
        const schema_2 = schema_1.unpublish('me', now);

        expect(schema_2.isPublished).toBeFalsy();
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update properties property and user info when updating', () => {
        const newProperties = new SchemaPropertiesDto('New Name', null);

        const now = DateTime.now();

        const schema_1 = new SchemaDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null);
        const schema_2 = schema_1.update(newProperties, 'me', now);

        expect(schema_2.properties).toEqual(newProperties);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });
});

describe('SchemaDetailsDto', () => {
    const properties = new SchemaPropertiesDto('Name', null);

    it('should update isPublished property and user info when publishing', () => {
        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, []);
        const schema_2 = schema_1.publish('me', now);

        expect(schema_2.isPublished).toBeTruthy();
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update isPublished property and user info when unpublishing', () => {
        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, true, 'other', 'other', DateTime.now(), DateTime.now(), null, []);
        const schema_2 = schema_1.unpublish('me', now);

        expect(schema_2.isPublished).toBeFalsy();
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update properties property and user info when updating', () => {
        const newProperties = new SchemaPropertiesDto('New Name', null);

        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, []);
        const schema_2 = schema_1.update(newProperties, 'me', now);

        expect(schema_2.properties).toEqual(newProperties);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update fields property and user info when adding field', () => {
        const field1 = new FieldDto(1, '1', false, false, 'l', createProperties('String'));
        const field2 = new FieldDto(2, '2', false, false, 'l', createProperties('Number'));

        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, [field1]);
        const schema_2 = schema_1.addField(field2, 'me', now);

        expect(schema_2.fields).toEqual([field1, field2]);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update fields property and user info when removing field', () => {
        const field1 = new FieldDto(1, '1', false, false, 'l', createProperties('String'));
        const field2 = new FieldDto(2, '2', false, false, 'l', createProperties('Number'));

        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, [field1, field2]);
        const schema_2 = schema_1.removeField(field1, 'me', now);

        expect(schema_2.fields).toEqual([field2]);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update fields property and user info when replacing fields', () => {
        const field1 = new FieldDto(1, '1', false, false, 'l', createProperties('String'));
        const field2 = new FieldDto(2, '2', false, false, 'l', createProperties('Number'));

        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, [field1, field2]);
        const schema_2 = schema_1.replaceFields([field2, field1], 'me', now);

        expect(schema_2.fields).toEqual([field2, field1]);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });

    it('should update fields property and user info when updating field', () => {
        const field1 = new FieldDto(1, '1', false, false, 'l', createProperties('String'));
        const field2_1 = new FieldDto(2, '2', false, false, 'l', createProperties('Number'));
        const field2_2 = new FieldDto(2, '2', false, false, 'l', createProperties('Boolean'));

        const now = DateTime.now();

        const schema_1 = new SchemaDetailsDto('1', 'name', properties, false, 'other', 'other', DateTime.now(), DateTime.now(), null, [field1, field2_1]);
        const schema_2 = schema_1.updateField(field2_2, 'me', now);

        expect(schema_2.fields).toEqual([field1, field2_2]);
        expect(schema_2.lastModified).toEqual(now);
        expect(schema_2.lastModifiedBy).toEqual('me');
    });
});

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
                LocalCacheService,
                SchemasService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
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

        let schemas: SchemaDto[] | null = null;

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

        expect(schemas).toEqual([
            new SchemaDto('id1', 'name1', new SchemaPropertiesDto('label1', 'hints1'), true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                new Version('11')),
            new SchemaDto('id2', 'name2', new SchemaPropertiesDto('label2', 'hints2'), true, 'Created2', 'LastModifiedBy2',
                DateTime.parseISO_UTC('2016-10-12T10:10'),
                DateTime.parseISO_UTC('2017-10-12T10:10'),
                new Version('22'))
        ]);
    }));

    it('should make get request to get schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        let schema: SchemaDetailsDto | null = null;

        schemasService.getSchema('my-app', 'my-schema', version).subscribe(result => {
            schema = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            name: 'name1',
            isPublished: true,
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            version: 11,
            properties: {
                label: 'label1',
                hints: 'hints1'
            },
            fields: [
                {
                    fieldId: 1,
                    name: 'field1',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Number'
                    }
                },
                {
                    fieldId: 2,
                    name: 'field2',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'String'
                    }
                },
                {
                    fieldId: 3,
                    name: 'field3',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Boolean'
                    }
                },
                {
                    fieldId: 4,
                    name: 'field4',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'DateTime'
                    }
                },
                {
                    fieldId: 5,
                    name: 'field5',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Json'
                    }
                },
                {
                    fieldId: 6,
                    name: 'field6',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Geolocation'
                    }
                },
                {
                    fieldId: 7,
                    name: 'field7',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'Assets'
                    }
                },
                {
                    fieldId: 8,
                    name: 'field8',
                    isHidden: true,
                    isDisabled: true,
                    partitioning: 'language',
                    properties: {
                        fieldType: 'References'
                    }
                }
            ]
        });

        expect(schema).toEqual(
            new SchemaDetailsDto('id1', 'name1', new SchemaPropertiesDto('label1', 'hints1'), true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                new Version('11'),
                [
                    new FieldDto(1, 'field1', true, true, 'language', createProperties('Number')),
                    new FieldDto(2, 'field2', true, true, 'language', createProperties('String')),
                    new FieldDto(3, 'field3', true, true, 'language', createProperties('Boolean')),
                    new FieldDto(4, 'field4', true, true, 'language', createProperties('DateTime')),
                    new FieldDto(5, 'field5', true, true, 'language', createProperties('Json')),
                    new FieldDto(6, 'field6', true, true, 'language', createProperties('Geolocation')),
                    new FieldDto(7, 'field7', true, true, 'language', createProperties('Assets')),
                    new FieldDto(8, 'field8', true, true, 'language', createProperties('References'))
                ]));
    }));

    it('should provide entry from cache if not found',
        inject([LocalCacheService, SchemasService, HttpTestingController], (localCache: LocalCacheService, schemasService: SchemasService, httpMock: HttpTestingController) => {

        const cached = {};

        localCache.set('schema.my-app.my-schema', cached, 10000);

        let schema: SchemaDetailsDto | null = null;

        schemasService.getSchema('my-app', 'my-schema', version).subscribe(result => {
            schema = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({}, { status: 404, statusText: '404' });

        expect(schema).toBe(cached);
    }));

    it('should make post request to create schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new CreateSchemaDto('name');

        let schema: SchemaDetailsDto | null = null;

        schemasService.postSchema('my-app', dto, user, now, version).subscribe(result => {
            schema = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: '1' });

        expect(schema).toEqual(
            new SchemaDetailsDto('1', dto.name, new SchemaPropertiesDto(null, null), false, user, user, now, now, version, []));
    }));

    it('should make post request to add field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new AddFieldDto('name', 'invariant', createProperties('Number'));

        let field: FieldDto | null = null;

        schemasService.postField('my-app', 'my-schema', dto, version).subscribe(result => {
            field = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 123 });

        expect(field).toEqual(
            new FieldDto(123, dto.name, false, false, dto.partitioning, dto.properties));
    }));

    it('should make put request to update schema',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateSchemaDto('label', 'hints');

        schemasService.putSchema('my-app', 'my-schema', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 123 });
    }));

    it('should make put request to update field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = new UpdateFieldDto(createProperties('Number'));

        schemasService.putField('my-app', 'my-schema', 1, dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to update field ordering',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        const dto = [1, 2, 3];

        schemasService.putFieldOrdering('my-app', 'my-schema', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/ordering');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
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

    it('should make put request to enable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.enableField('my-app', 'my-schema', 1, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/enable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to disable field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.disableField('my-app', 'my-schema', 1, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/disable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to show field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.showField('my-app', 'my-schema', 1, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/show');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to hide field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.hideField('my-app', 'my-schema', 1, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/hide');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make delete request to delete field',
        inject([SchemasService, HttpTestingController], (schemasService: SchemasService, httpMock: HttpTestingController) => {

        schemasService.deleteField('my-app', 'my-schema', 1, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/fields/1');

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