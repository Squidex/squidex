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
    UpdateSchemaDto
} from './../';

describe('SchemasService', () => {
    let authService: IMock<AuthService>;
    let schemasService: SchemasService;

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
            new SchemaDto('id1', 'name1', 'label1', true, 'Created1', 'LastModifiedBy1', DateTime.parseISO_UTC('2016-12-12T10:10'), DateTime.parseISO_UTC('2017-12-12T10:10')),
            new SchemaDto('id2', 'name2', 'label2',  true, 'Created2', 'LastModifiedBy2', DateTime.parseISO_UTC('2016-10-12T10:10'), DateTime.parseISO_UTC('2017-10-12T10:10'))
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
                            fields: [{
                                fieldId: 123,
                                name: 'field1',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'number'
                                }
                            }, {
                                fieldId: 234,
                                name: 'field2',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'string'
                                }
                            }, {
                                fieldId: 345,
                                name: 'field3',
                                isHidden: true,
                                isDisabled: true,
                                properties: {
                                    fieldType: 'boolean'
                                }
                            }]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let schema: SchemaDetailsDto | null = null;

        schemasService.getSchema('my-app', 'my-schema').subscribe(result => {
            schema = result;
        }).unsubscribe();

        expect(schema).toEqual(
            new SchemaDetailsDto('id1', 'name1', 'label1', 'hints1', true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'), [
                    new FieldDto(123, 'field1', true, true, createProperties('number')),
                    new FieldDto(234, 'field2', true, true, createProperties('string')),
                    new FieldDto(345, 'field3', true, true, createProperties('boolean'))
                ]));

        authService.verifyAll();
    });


    it('should make post request to create schema', () => {
        const dto = new CreateSchemaDto('name');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/schemas', dto))
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

        schemasService.postSchema('my-app', dto).subscribe(result => {
            created = result;
        });

        expect(created).toEqual(
            new EntityCreatedDto('my-schema'));

        authService.verifyAll();
    });

    it('should make post request to add field', () => {
        const dto = new AddFieldDto('name', createProperties('number'));

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/schemas/my-schema/fields', dto))
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

        schemasService.postField('my-app', 'my-schema', dto).subscribe(result => {
            created = result;
        });

        expect(created).toEqual(
            new EntityCreatedDto(123));

        authService.verifyAll();
    });

    it('should make put request to update schema', () => {
        const dto = new UpdateSchemaDto('label', 'hints');

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema', dto))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.putSchema('my-app', 'my-schema', dto);

        authService.verifyAll();
    });

    it('should make put request to update field', () => {
        const dto = new UpdateFieldDto(createProperties('number'));

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1', dto))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.putField('my-app', 'my-schema', 1, dto);

        authService.verifyAll();
    });

    it('should make put request to publish schema', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/publish', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.publishSchema('my-app', 'my-schema');

        authService.verifyAll();
    });

    it('should make put request to unpublish schema', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/unpublish', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.unpublishSchema('my-app', 'my-schema');

        authService.verifyAll();
    });

    it('should make put request to enable field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/enable', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.enableField('my-app', 'my-schema', 1);

        authService.verifyAll();
    });

    it('should make put request to disable field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/disable', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.disableField('my-app', 'my-schema', 1);

        authService.verifyAll();
    });

    it('should make put request to show field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/show', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.showField('my-app', 'my-schema', 1);

        authService.verifyAll();
    });

    it('should make put request to hide field', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/schemas/my-schema/fields/1/hide', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.hideField('my-app', 'my-schema', 1);

        authService.verifyAll();
    });

    it('should make delete request to delete field', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/schemas/my-schema/fields/1'))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        schemasService.deleteField('my-app', 'my-schema', 1);

        authService.verifyAll();
    });
});