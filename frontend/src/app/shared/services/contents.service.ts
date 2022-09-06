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
import { ApiUrlConfig, DateTime, ErrorDto, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';
import { StatusInfo } from './../state/contents.state';
import { Query, sanitize } from './query';
import { parseField, RootFieldDto } from './schemas.service';

export class ScheduleDto {
    constructor(
        public readonly status: string,
        public readonly scheduledBy: string,
        public readonly color: string,
        public readonly dueTime: DateTime,
    ) {
    }
}

export class ContentDto {
    public readonly _links: ResourceLinks;

    public readonly statusUpdates: ReadonlyArray<StatusInfo>;

    public readonly canDelete: boolean;
    public readonly canDraftDelete: boolean;
    public readonly canDraftCreate: boolean;
    public readonly canCancelStatus: boolean;
    public readonly canUpdate: boolean;

    public get canPublish() {
        return this.statusUpdates.find(x => x.status === 'Published');
    }

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
        public readonly status: string,
        public readonly statusColor: string,
        public readonly newStatus: string | undefined,
        public readonly newStatusColor: string | undefined,
        public readonly scheduleJob: ScheduleDto | null | undefined,
        public readonly data: ContentData,
        public readonly schemaName: string,
        public readonly schemaDisplayName: string,
        public readonly referenceData: ContentReferences,
        public readonly referenceFields: ReadonlyArray<RootFieldDto>,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDraftCreate = hasAnyLink(links, 'draft/create');
        this.canDraftDelete = hasAnyLink(links, 'draft/delete');
        this.canCancelStatus = hasAnyLink(links, 'cancel');
        this.canUpdate = hasAnyLink(links, 'update');

        const updates: StatusInfo[] = [];

        for (const [key, link] of Object.entries(links)) {
            if (key.startsWith('status/')) {
                updates.push({ status: key.substring(7), color: link.metadata! });
            }
        }

        this.statusUpdates = updates;
    }
}

export class BulkResultDto {
    constructor(
        public readonly contentId: string,
        public readonly error?: ErrorDto,
    ) {
    }
}

export type ContentsDto = Readonly<{
    // The list of content items.
    items: ReadonlyArray<ContentDto>;

    // The total number of content items.
    total: number;

    // The statuses.
    statuses: ReadonlyArray<StatusInfo>;

    // True, if the user has permissions to create a content item.
    canCreate?: boolean;

    // True, if the user has permissions to create and publish a content item.
    canCreateAndPublish?: boolean;
}>;

export type ContentReferencesValue = Readonly<{
    // The references by partition.
    [partition: string]: string;
}> | string;

export type ContentReferences = Readonly<{
    // The reference values by field name.
    [fieldName: string ]: ContentFieldData<ContentReferencesValue>;
}>;

export type ContentFieldData<T = any> = Readonly<{
    // The data by partition.
    [partition: string]: T;
}>;

export type ContentData = Readonly<{
    // The content data by field name.
    [fieldName: string ]: ContentFieldData;
}>;

export type BulkStatusDto = Readonly<{
}>;

export type BulkUpdateDto = Readonly<{
    // The list of bulk update jobs.
    jobs: ReadonlyArray<BulkUpdateJobDto>;

    // True, if scripts should not be executed.
    doNotScript?: boolean;

    // True, if referrers should be checked.
    checkReferrers?: boolean;
}>;

export type BulkUpdateJobDto = Readonly<{
    // The ID of the content to update.
    id: string;

    // The type of the bulk update job.
    type: 'Upsert' | 'ChangeStatus' | 'Delete' | 'Validate';

    // The schema of the content item.
    schema?: string;

    // The new status.
    status?: string;

    // The due time of the new status.
    dueTime?: string | null;

    // The expected version of the content.
    expectedVersion?: number;
}>;

export type ContentsQuery = Readonly<{
    // True, to not return the total number of items.
    noTotal?: boolean;

    // True, to not return the total number of items, if the query would be slow.
    noSlowTotal?: boolean;
}>;

export type ContentsByIds = Readonly<{
    // The Ids of the contents to query.
    ids: ReadonlyArray<string>;
}> & ContentsQuery;

export type ContentsBySchedule = Readonly<{
    // The start of the time frame for scheduled content items.
    scheduledFrom: string | null;

    // The end of the time frame for scheduled content items.
    scheduledTo: string | null;
}> & ContentsQuery;

export type ContentsByQuery = Readonly<{
    // The JSON query.
    query?: Query;

    // The number of items to skip.
    skip?: number;

    // The number of items to take.
    take?: number;
}> & ContentsQuery;

@Injectable()
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getContents(appName: string, schemaName: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const body = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/query`);

        return this.http.post<any>(url, body, buildHeaders(q, false)).pipe(
            map(body => {
                return parseContents(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContent(appName: string, schemaName: string, id: string, language?: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        let headers = new HttpHeaders();

        if (language) {
            headers = headers.set('X-Flatten', '1');
            headers = headers.set('X-Languages', language);
        }

        return HTTP.getVersioned(this.http, url, undefined, headers).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.loadContentFailed'));
    }

    public getAllContents(appName: string, q: ContentsByIds | ContentsBySchedule): Observable<ContentsDto> {
        const { ...body } = q;

        const url = this.apiUrl.buildUrl(`/api/content/${appName}`);

        return this.http.post<any>(url, body, buildHeaders(q, false)).pipe(
            map(body => {
                return parseContents(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferences(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const { fullQuery } = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/references?${fullQuery}`);

        return this.http.get<any>(url, buildHeaders(q, false)).pipe(
            map(body => {
                return parseContents(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferencing(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const { fullQuery } = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/referencing?${fullQuery}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseContents(body);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version.value}`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return body;
            }),
            pretifyError('i18n:contents.loadDataFailed'));
    }

    public postContent(appName: string, schemaName: string, data: any, publish: boolean, id = ''): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?publish=${publish}&id=${id ?? ''}`);

        return HTTP.postVersioned(this.http, url, data).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.createFailed'));
    }

    public putContent(appName: string, resource: Resource, dto: any, version: Version): Observable<ContentDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public patchContent(appName: string, resource: Resource, dto: any, version: Version): Observable<ContentDto> {
        const link = resource._links['patch'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public createVersion(appName: string, resource: Resource, version: Version): Observable<ContentDto> {
        const link = resource._links['draft/create'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.loadVersionFailed'));
    }

    public cancelStatus(appName: string, resource: Resource, version: Version): Observable<ContentDto> {
        const link = resource._links['cancel'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.updateFailed'));
    }

    public deleteVersion(appName: string, resource: Resource, version: Version): Observable<ContentDto> {
        const link = resource._links['draft/delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.deleteVersionFailed'));
    }

    public bulkUpdate(appName: string, schemaName: string, dto: BulkUpdateDto): Observable<ReadonlyArray<BulkResultDto>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/bulk`);

        return this.http.post<any[]>(url, dto).pipe(
            map(body => {
                return body.map(x => new BulkResultDto(x.contentId, parseError(x.error)));
            }),
            pretifyError('i18n:contents.bulkFailed'));
    }
}

function buildHeaders(q: ContentsQuery | undefined, noTotal: boolean) {
    let options = {
        headers: {},
    };

    if (q?.noTotal || noTotal) {
        options.headers['X-NoTotal'] = '1';
    }

    if (q?.noSlowTotal) {
        options.headers['X-NoSlowTotal'] = '1';
    }

    return options;
}

function buildQuery(q?: ContentsByQuery) {
    const { query, skip, take } = q || {};

    const body: any = {};

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

        body.odata = odataParts.join('&');
    } else {
        const queryObj: Query = { ...query };

        if (take && take > 0) {
            queryObj.take = take;
        }

        if (skip && skip > 0) {
            queryObj.skip = skip;
        }

        body.q = sanitize(queryObj);
    }

    return body;
}

function parseContents(response: { items: any[]; total: number; statuses: any } & Resource): ContentsDto {
    const { items: list, total, statuses, _links } = response;
    const items = list.map(parseContent);

    const canCreate = hasAnyLink(_links, 'create');
    const canCreateAndPublish = hasAnyLink(_links, 'create/publish');

    return { items, total, statuses, canCreate, canCreateAndPublish };
}

function parseContent(response: any) {
    return new ContentDto(response._links,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.status,
        response.statusColor,
        response.newStatus,
        response.newStatusColor,
        parseScheduleJob(response.scheduleJob),
        response.data,
        response.schemaName,
        response.schemaDisplayName,
        response.referenceData,
        response.referenceFields.map(parseField));
}

function parseScheduleJob(response: any) {
    if (!response) {
        return null;
    }

    return new ScheduleDto(
        response.status,
        response.scheduledBy,
        response.color,
        DateTime.parseISO(response.dueTime));
}

function parseError(response: any) {
    if (!response) {
        return undefined;
    }

    return new ErrorDto(
        response.statusCode,
        response.message,
        response.errorCode,
        response.details);
}
