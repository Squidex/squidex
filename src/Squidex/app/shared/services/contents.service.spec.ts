/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    EntityCreatedDto,
    ContentDto,
    ContentsDto,
    ContentsService,
    DateTime,
    Version
} from './../';

describe('ContentsService', () => {
    let authService: IMock<AuthService>;
    let contentsService: ContentsService;
    let version = new Version('1');

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        contentsService = new ContentsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get contents', () => {
        authService.setup(x => x.authGet('http://service/p/api/content/my-app/my-schema?nonPublished=true&hidden=true&$top=17&$skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: [{
                                id: 'id1',
                                isPublished: true,
                                created: '2016-12-12T10:10',
                                createdBy: 'Created1',
                                lastModified: '2017-12-12T10:10',
                                lastModifiedBy: 'LastModifiedBy1',
                                version: 11,
                                data: {}
                            }, {
                                id: 'id2',
                                isPublished: true,
                                created: '2016-10-12T10:10',
                                createdBy: 'Created2',
                                lastModified: '2017-10-12T10:10',
                                lastModifiedBy: 'LastModifiedBy2',
                                version: 22,
                                data: {}
                            }]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, null).subscribe(result => {
            contents = result;
        }).unsubscribe();

        expect(contents).toEqual(
            new ContentsDto(10, [
                new ContentDto('id1', true, 'Created1', 'LastModifiedBy1',
                    DateTime.parseISO_UTC('2016-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    {},
                    new Version('11')),
                new ContentDto('id2', true, 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    {},
                    new Version('22'))
        ]));

        authService.verifyAll();
    });

    it('should append query to get request as search', () => {
        authService.setup(x => x.authGet('http://service/p/api/content/my-app/my-schema?nonPublished=true&hidden=true&$search=my-query&$top=17&$skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: []
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, 'my-query').subscribe(result => {
            contents = result;
        }).unsubscribe();
        authService.verifyAll();
    });

    it('should append query to get request as plain query string', () => {
        authService.setup(x => x.authGet('http://service/p/api/content/my-app/my-schema?nonPublished=true&hidden=true&$filter=my-filter&$top=17&$skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: []
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, '$filter=my-filter').subscribe(result => {
            contents = result;
        }).unsubscribe();
        authService.verifyAll();
    });

    it('should make get request to get content', () => {
        authService.setup(x => x.authGet('http://service/p/api/content/my-app/my-schema/content1?hidden=true', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            id: 'id1',
                            isPublished: true,
                            created: '2016-12-12T10:10',
                            createdBy: 'Created1',
                            lastModified: '2017-12-12T10:10',
                            lastModifiedBy: 'LastModifiedBy1',
                            version: 11,
                            data: {}
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let content: ContentDto | null = null;

        contentsService.getContent('my-app', 'my-schema', 'content1', version).subscribe(result => {
            content = result;
        }).unsubscribe();

        expect(content).toEqual(
            new ContentDto('id1', true, 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                {},
                new Version('11')));

        authService.verifyAll();
    });

    it('should make post request to create content', () => {
        const dto = {};

        authService.setup(x => x.authPost('http://service/p/api/content/my-app/my-schema', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: 'content1'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let created: EntityCreatedDto | null = null;

        contentsService.postContent('my-app', 'my-schema', dto, version).subscribe(result => {
            created = result;
        });

        expect(created).toEqual(
            new EntityCreatedDto('content1'));

        authService.verifyAll();
    });

    it('should make put request to update content', () => {
        const dto = {};

        authService.setup(x => x.authPut('http://service/p/api/content/my-app/my-schema/content1', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        contentsService.putContent('my-app', 'my-schema', 'content1', dto, version);

        authService.verifyAll();
    });

    it('should make put request to publish content', () => {
        authService.setup(x => x.authPut('http://service/p/api/content/my-app/my-schema/content1/publish', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        contentsService.publishContent('my-app', 'my-schema', 'content1', version);

        authService.verifyAll();
    });

    it('should make put request to unpublish content', () => {
        authService.setup(x => x.authPut('http://service/p/api/content/my-app/my-schema/content1/unpublish', It.isAny(), version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        contentsService.unpublishContent('my-app', 'my-schema', 'content1', version);

        authService.verifyAll();
    });

    it('should make delete request to delete content', () => {
        authService.setup(x => x.authDelete('http://service/p/api/content/my-app/my-schema/content1', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        contentsService.deleteContent('my-app', 'my-schema', 'content1', version);

        authService.verifyAll();
    });
});