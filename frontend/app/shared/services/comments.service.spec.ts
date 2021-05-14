/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, CommentDto, CommentsDto, CommentsService, DateTime, Version } from '@app/shared/internal';

describe('CommentsService', () => {
    const user = 'me';

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                CommentsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get comments',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {
            let comments: CommentsDto;

            commentsService.getComments('my-comments', new Version('123')).subscribe(result => {
                comments = result;
            });

            const req = httpMock.expectOne('http://service/p/api/my-comments?version=123');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-Silent')).toBe('1');

            req.flush({
                createdComments: [{
                    id: '123',
                    text: 'text1',
                    time: '2016-10-12T10:10',
                    user,
                }],
                updatedComments: [{
                    id: '456',
                    text: 'text2',
                    time: '2017-11-12T12:12',
                    user,
                }],
                deletedComments: ['789'],
                version: '9',
            });

            expect(comments!).toEqual(
                new CommentsDto(
                    [
                        new CommentDto('123', DateTime.parseISO('2016-10-12T10:10Z'), 'text1', undefined, user),
                    ], [
                        new CommentDto('456', DateTime.parseISO('2017-11-12T12:12Z'), 'text2', undefined, user),
                    ], [
                        '789',
                    ],
                    new Version('9')),
            );
        }));

    it('should make post request to create comment',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {
            const dto = { text: 'text1' };

            let comment: CommentDto;

            commentsService.postComment('my-comments', dto).subscribe(result => {
                comment = result;
            });

            const req = httpMock.expectOne('http://service/p/api/my-comments');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                id: '123',
                text: 'text1',
                time: '2016-10-12T10:10',
                user,
            });

            expect(comment!).toEqual(new CommentDto('123', DateTime.parseISO('2016-10-12T10:10Z'), 'text1', undefined, user));
        }));

    it('should make put request to replace comment content',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {
            const dto = { text: 'text1' };

            commentsService.putComment('my-comments', '123', dto).subscribe();

            const req = httpMock.expectOne('http://service/p/api/my-comments/123');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to delete comment',
        inject([CommentsService, HttpTestingController], (commentsService: CommentsService, httpMock: HttpTestingController) => {
            commentsService.deleteComment('my-comments', '123').subscribe();

            const req = httpMock.expectOne('http://service/p/api/my-comments/123');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));
});
