/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, DateTime, Model, pretifyError, Version } from '@app/framework';

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

export type UpsertCommentDto = Readonly<{
    // The text to comment.
    text: string;

    // The url to the comment.
    url?: string;
}>;

@Injectable()
export class CommentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getWatchingUsers(appId: string, resource: string): Observable<ReadonlyArray<string>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appId}/watching/${resource}`);

        const options = {
            headers: new HttpHeaders({
                'X-Silent': '1',
            }),
        };

        return this.http.get<ReadonlyArray<string>>(url, options);
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
                return parseComments(body);
            }),
            pretifyError('i18n:comments.loadFailed'));
    }

    public postComment(commentsUrl: string, dto: UpsertCommentDto): Observable<CommentDto> {
        const url = this.apiUrl.buildUrl(`api/${commentsUrl}`);

        return this.http.post<any>(url, dto).pipe(
            map(body => {
                return parseComment(body);
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
function parseComments(response: any) {
    return new CommentsDto(
        response.createdComments.map(parseComment),
        response.updatedComments.map(parseComment),
        response.deletedComments,
        new Version(response.version));
}

function parseComment(response: any) {
    return new CommentDto(
        response.id,
        DateTime.parseISO(response.time),
        response.text,
        response.url,
        response.user);
}

