/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    CommentDto,
    CommentsDto,
    CommentsService,
    DateTime,
    UpsertCommentDto,
    Version
} from './../';

describe('CommentsService', () => {
    const user = 'me';

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                CommentsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get comments',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {

        let comments: CommentsDto;

        commentsService.getComments('my-app', 'my-comments', new Version('123')).subscribe(result => {
            comments = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/comments/my-comments?version=123');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            createdComments: [{
                id: '123',
                text: 'text1',
                time: '2016-10-12T10:10',
                user: user
            }],
            updatedComments: [{
                id: '456',
                text: 'text2',
                time: '2017-11-12T12:12',
                user: user
            }],
            deletedComments: ['789'],
            version: '9'
        });

        expect(comments!).toEqual(
            new CommentsDto(
                [
                    new CommentDto('123', DateTime.parseISO_UTC('2016-10-12T10:10'), 'text1', user)
                ], [
                    new CommentDto('456', DateTime.parseISO_UTC('2017-11-12T12:12'), 'text2', user)
                ], [
                    '789'
                ],
                new Version('9'))
        );
    }));

    it('should make post request to create comment',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {

        let comment: CommentDto;

        commentsService.postComment('my-app', 'my-comments', new UpsertCommentDto('text1')).subscribe(result => {
            comment = <CommentDto>result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/comments/my-comments');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: '123',
            text: 'text1',
            time: '2016-10-12T10:10',
            user: user
        });

        expect(comment!).toEqual(new CommentDto('123', DateTime.parseISO_UTC('2016-10-12T10:10'), 'text1', user));
    }));

    it('should make put request to replace comment content',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {

        commentsService.putComment('my-app', 'my-comments', '123', new UpsertCommentDto('text1')).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/comments/my-comments/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make delete request to delete comment',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {

        commentsService.deleteComment('my-app', 'my-comments', '123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/comments/my-comments/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});