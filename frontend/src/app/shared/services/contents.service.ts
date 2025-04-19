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
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, StringHelper, Version, Versioned, VersionOrTag } from '@app/framework';
import { BulkResultDto, BulkUpdateContentsDto, ContentDto, ContentsDto } from './../model';
import { Query, sanitize } from './query';

export type StatusInfo = Readonly<{ status: string; color: string }>;

export type ContentsQuery = Readonly<{
    // True, to not return the total number of items.
    noTotal?: boolean;

    // True, to not return the total number of items, if the query would be slow.
    noSlowTotal?: boolean;

    // The field names to query.
    fieldNames?: ReadonlyArray<string>;
}>;

export type ContentsByIds = Readonly<{
    // The Ids of the contents to query.
    ids: ReadonlyArray<string>;
}>;

export type ContentsBySchedule = Readonly<{
    // The start of the time frame for scheduled content items.
    scheduledFrom: string | null;

    // The end of the time frame for scheduled content items.
    scheduledTo: string | null;
}>;

export type ContentsByReferences = Readonly<{
    // The reference content id.
    references: string;
}>;

export type ContentsByReferencing = Readonly<{
    // The referencing content id.
    referencing: string;
}>;

export type ContentsByQuery = Readonly<{
    // The JSON query.
    query?: Query;

    // The number of items to skip.
    skip?: number;

    // The number of items to take.
    take?: number;
}> & ContentsQuery;

type FullQuery = ContentsByIds | ContentsBySchedule | ContentsByReferences | ContentsByReferencing;

@Injectable({
    providedIn: 'root',
})
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getContents(appName: string, schemaName: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const body = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/query`);

        return this.http.post<any>(url, body, buildHeaders(q)).pipe(
            map(body => {
                return ContentsDto.fromJSON(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getAllContents(appName: string, primary: FullQuery, q?: ContentsByQuery): Observable<ContentsDto> {
        const body = buildFullQuery(primary, q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}`);

        return this.http.post<any>(url, body, buildHeaders(q)).pipe(
            map(body => {
                return ContentsDto.fromJSON(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferences(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const query = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/references${buildQueryString(query)}`);

        return this.http.get<any>(url, buildHeaders(q)).pipe(
            map(body => {
                return ContentsDto.fromJSON(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferencing(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const query = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/referencing${buildQueryString(query)}`);

        return this.http.get<any>(url, buildHeaders(q)).pipe(
            map(body => {
                return ContentsDto.fromJSON(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.loadContentFailed'));
    }

    public getRawContent(appName: string, schemaName: string, id: string, language?: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        let headers = new HttpHeaders();

        if (language) {
            headers = headers.set('X-Flatten', '1');
            headers = headers.set('X-Languages', language);
        }

        return HTTP.getVersioned(this.http, url, undefined, headers).pipe(
            map(({ payload }) => {
                return payload.body;
            }),
            pretifyError('i18n:contents.loadContentFailed'));
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version}`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return body;
            }),
            pretifyError('i18n:contents.loadDataFailed'));
    }

    public postContent(appName: string, schemaName: string, data: any, publish: boolean, id = ''): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}${StringHelper.buildQuery({ publish, id })}`);

        return HTTP.postVersioned(this.http, url, data).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.createFailed'));
    }

    public putContent(appName: string, resource: Resource, dto: any, version: VersionOrTag): Observable<ContentDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public patchContent(appName: string, resource: Resource, dto: any, version: VersionOrTag): Observable<ContentDto> {
        const link = resource._links['patch'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public createVersion(appName: string, resource: Resource, version: VersionOrTag): Observable<ContentDto> {
        const link = resource._links['draft/create'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.loadVersionFailed'));
    }

    public cancelStatus(appName: string, resource: Resource, version: VersionOrTag): Observable<ContentDto> {
        const link = resource._links['cancel'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public deleteVersion(appName: string, resource: Resource, version: VersionOrTag): Observable<ContentDto> {
        const link = resource._links['draft/delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return ContentDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:contents.deleteVersionFailed'));
    }

    public bulkUpdate(appName: string, schemaName: string, dto: BulkUpdateContentsDto): Observable<ReadonlyArray<BulkResultDto>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/bulk`);

        return this.http.post<any[]>(url, dto).pipe(
            map(body => {
                return body.map(BulkResultDto.fromJSON);
            }),
            pretifyError('i18n:contents.bulkFailed'));
    }
}

function buildHeaders(q: ContentsQuery | undefined) {
    let options = {
        headers: {} as Record<string, string>,
    };

    if (q?.fieldNames) {
        options.headers['X-Fields'] = q.fieldNames.join(',');
    }

    if (q?.noTotal) {
        options.headers['X-NoTotal'] = '1';
    }

    if (q?.noSlowTotal) {
        options.headers['X-NoSlowTotal'] = '1';
    }

    return options;
}

function buildFullQuery(primary: FullQuery, q?: ContentsByQuery) {
    const query = buildQuery(q);

    return { ...query, ...primary };
}

function buildQueryString(input: { q?: object; odata?: string }) {
    const { odata, q } = input;

    return q ? `?q=${JSON.stringify(q)}` : `?${odata}`;
}

function buildQuery(q?: ContentsByQuery): { q?: object; odata?: string } {
    const { query, skip, take } = q || {};

    if (query && query.fullText && query.fullText.indexOf('$') >= 0) {
        const odataParts: string[] = [
            `${query.fullText.trim()}`,
        ];

        if (take && take > 0) {
            odataParts.push(`$top=${take}`);
        }

        if (skip && skip > 0) {
            odataParts.push(`$skip=${skip}`);
        }

        return { odata: odataParts.join('&') };
    } else {
        const queryObj: Query = { ...query };

        if (take && take > 0) {
            queryObj.take = take;
        }

        if (skip && skip > 0) {
            queryObj.skip = skip;
        }

        return { q: sanitize(queryObj) };
    }
}