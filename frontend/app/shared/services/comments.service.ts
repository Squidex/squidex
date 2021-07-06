/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, DateTime, Model, pretifyError, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class CommentsDto extends Model<CommentsDto> {
    constructor(
        public readonly createdComments: ReadonlyArray<CommentDto>,
        public readonly updatedComments: ReadonlyArray<CommentDto>,
        public readonly deletedComments: ReadonlyArray<string>,
        public readonly version: Version,
    ) {
        super();
    }
}

export class CommentDto extends Model<CommentDto> {
    constructor(
        public readonly id: string,
        public readonly time: DateTime,
        public readonly text: string,
        public readonly url: string | undefined,
        public readonly user: string,
    ) {
        super();
    }
}

export type UpsertCommentDto =
    Readonly<{ text: string; url?: string }>;

@Injectable()
export class CommentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getComments(commentsUrl: string, version: Version): Observable<CommentsDto> {
        const url = this.apiUrl.buildUrl(`api/${commentsUrl}?version=${version.value}`);

        const options = {
            headers: new HttpHeaders({
                'X-Silent': '1',
            }),
        };

        return this.http.get<any>(url, options).pipe(
            map(body => {
                const comments = new CommentsDto(
                    body.createdComments.map((item: any) => {
                        return new CommentDto(
                            item.id,
                            DateTime.parseISO(item.time),
                            item.text,
                            item.url,
                            item.user);
                    }),
                    body.updatedComments.map((item: any) => {
                        return new CommentDto(
                            item.id,
                            DateTime.parseISO(item.time),
                            item.text,
                            item.url,
                            item.user);
                    }),
                    body.deletedComments,
                    new Version(body.version),
                );

                return comments;
            }),
            pretifyError('i18n:comments.loadFailed'));
    }

    public postComment(commentsUrl: string, dto: UpsertCommentDto): Observable<CommentDto> {
        const url = this.apiUrl.buildUrl(`api/${commentsUrl}`);

        return this.http.post<any>(url, dto).pipe(
            map(body => {
                const comment = new CommentDto(
                    body.id,
                    DateTime.parseISO(body.time),
                    body.text,
                    body.url,
                    body.user);

                return comment;
            }),
            pretifyError('i18n:comments.createFailed'));
    }

    public putComment(commentsUrl: string, commentId: string, dto: UpsertCommentDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/${commentsUrl}/${commentId}`);

        return this.http.put(url, dto).pipe(
            pretifyError('i18n:comments.updateFailed'));
    }

    public deleteComment(commentsUrl: string, commentId: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/${commentsUrl}/${commentId}`);

        return this.http.delete(url).pipe(
            pretifyError('i18n:comments.deleteFailed'));
    }
}
