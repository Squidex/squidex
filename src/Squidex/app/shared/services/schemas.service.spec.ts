/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { It, IMock, Mock, Times } from 'typemoq';

import {
    AddFieldDto,
    ApiUrlConfig,
    AuthService,
    CreateSchemaDto,
    createProperties,
    DateTime,
    EntityCreatedDto,
    FieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaDto,
    Version
} from './../';

describe('SchemasService', () => {
    let authService: IMock<AuthService>;
    let schemasService: SchemasService;
    let version = new Version('1');

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        schemasService = new SchemasService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should throw if creating invalid property type', () => {
        expect(() => createProperties('invalid')).toThrow('Invalid properties type');
    });

    it('should make get request to get schemas', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/schemas'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            id: 'id1',
                            name: 'name1',
                            label: 'label1',
                            isPublished: true,
                            created: '2016-12-12T10:10',
                            createdBy: 'Created1',
                            lastModified: '2017-12-12T10:10',
                            lastModifiedBy: 'LastModifiedBy1',
                            version: 11,
                            data: {}
                        }, {
                            id: 'id2',
                            name: 'name2',
                            label: 'label2',
                            isPublished: true,
                            created: '2016-10-12T10:10',
                            createdBy: 'Created2',
                            lastModified: '2017-10-12T10:10',
                            lastModifiedBy: 'LastModifiedBy2',
                            version: 22,
                            data: {}
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let schemas: SchemaDto[] | null = null;

        schemasService.getSchemas('my-app').subscribe(result => {
            schemas = result;
        }).unsubscribe();

        expect(schemas).toEqual([
            new SchemaDto('id1', 'name1', 'label1', true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                new Version('11')),
            new SchemaDto('id2', 'name2', 'label2', true, 'Created2', 'LastModifiedBy2',
                DateTime.parseISO_UTC('2016-10-12T10:10'),
                DateTime.parseISO_UTC('2017-10-12T10:10'),
                new Version('22'))
        ]);

        authService.verifyAll();
    });

    it('should make get request to get schema', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/schemas/my-schema'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            id: 'id1',
                            name: 'name1',
                            label: 'label1',
                            hints: 'hints1',
                            isPublished: true,
                            created: '2016-12-12T10:10',
                            createdBy: 'Created1',
                            lastModified: '2017-12-12T10:10',
                            lastModifiedBy: 'LastModifiedBy1',
                            version: 11,
                            fields: [{
                                fieldId: 1,
                                name: 'field1',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'Number'
                                }
                            }, {
                                fieldId: 2,
                                name: 'field2',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'String'
                                }
                            }, {
                                fieldId: 3,
                                name: 'field3',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'Boolean'
                                }
                            }, {
                                fieldId: 4,
                                name: 'field4',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'DateTime'
                                }
                            }, {
                                fieldId: 5,
                                name: 'field5',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'Json'
                                }
                            }, {
                                fieldId: 6,
                                name: 'field6',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'Geolocation'
                                }
                            }, {
                                fieldId: 7,
                                name: 'field7',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'Assets'
                                }
                            }]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let schema: SchemaDetailsDto | null = null;

        schemasService.getSchema('my-app', 'my-schema', version).subscribe(result => {
            schema = result;
        }).unsubscribe();

        expect(schema).toEqual(
            new SchemaDetailsDto('id1', 'name1', 'label1', 'hints1', true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                new Version('11'),
                [
                    new FieldDto(1, 'field1', true, true, createProperties('Number')),
                    new FieldDto(2, 'field2', true, true, createProperties('String')),
                    new FieldDto(3, 'field3', true, true, createProperties('Boolean')),
                    new FieldDto(4, 'field4', true, true, createProperties('DateTime')),
                    new FieldDto(5, 'field5', true, true, createProperties('Json')),
                    new FieldDto(6, 'field6', true, true, createProperties('Geolocation')),
                    new FieldDto(7, 'field7', true, true, createProperties('Assets'))
                ]));

        authService.verifyAll();
    });

    it('should make post request to create schema', () => {
        const dto = new CreateSchemaDto('name');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/schemas', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: 'my-schema'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let created: EntityCreatedDto | null = null;

        schemasService.postSchema('my-app', dto, version).subscribe(result => {
            created = result;
        });

        expect(created).toEqual(
            new EntityCreatedDto('my-schema'));

        authService.verifyAll();
    });

    it('should make post request to add field', () => {
        const dto = new AddFieldDto('name', createProperties('Number'));

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/schemas/my-schema/fields', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: 123
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let created: EntityCreatedDto | null = null;

        schemasService.postField('my-app', 'my-schema', dto, version).subscribe(result => {
            created = result;
        });

        expect(created).toEqual(
            new EntityCreatedDto(123));

        authService.verifyAll();
    });

    it('should make put request to update schema', () => {
        const dto = new UpdateSchemaDto('label', 'hints');

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.putSchema('my-app', 'my-schema', dto, version);

        authService.verifyAll();
    });

    it('should make put request to update field', () => {
        const dto = new UpdateFieldDto(createProperties('Number'));

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.putField('my-app', 'my-schema', 1, dto, version);

        authService.verifyAll();
    });

    it('should make put request to update field ordering', () => {
        const dto = [1, 2, 3];

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/ordering', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.putFieldOrdering('my-app', 'my-schema', dto, version);

        authService.verifyAll();
    });

    it('should make put request to publish schema', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/publish', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.publishSchema('my-app', 'my-schema', version);

        authService.verifyAll();
    });

    it('should make put request to unpublish schema', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/unpublish', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.unpublishSchema('my-app', 'my-schema', version);

        authService.verifyAll();
    });

    it('should make put request to enable field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/enable', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.enableField('my-app', 'my-schema', 1, version);

        authService.verifyAll();
    });

    it('should make put request to disable field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/disable', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.disableField('my-app', 'my-schema', 1, version);

        authService.verifyAll();
    });

    it('should make put request to show field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/show', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.showField('my-app', 'my-schema', 1, version);

        authService.verifyAll();
    });

    it('should make put request to hide field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/hide', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.hideField('my-app', 'my-schema', 1, version);

        authService.verifyAll();
    });

    it('should make delete request to delete field', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/schemas/my-schema/fields/1', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.deleteField('my-app', 'my-schema', 1, version);

        authService.verifyAll();
    });

    it('should make delete request to delete schema', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/schemas/my-schema', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.deleteSchema('my-app', 'my-schema', version);

        authService.verifyAll();
    });
});