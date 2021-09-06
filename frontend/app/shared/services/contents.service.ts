/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, ErrorDto, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, ResultSet, Version, Versioned } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { encodeQuery, Query, StatusInfo } from './../state/query';
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

export class ContentsDto extends ResultSet<ContentDto> {
    constructor(
        public readonly statuses: ReadonlyArray<StatusInfo>,
        total: number,
        items: ReadonlyArray<ContentDto>,
        links?: ResourceLinks,
    ) {
        super(total, items, links);
    }

    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }

    public get canCreateAndPublish() {
        return hasAnyLink(this._links, 'create/publish');
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
        public readonly scheduleJob: ScheduleDto | null,
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

        for (const link in links) {
            if (links.hasOwnProperty(link) && link.startsWith('status/')) {
                const status = link.substr(7);

                updates.push({ status, color: links[link].metadata! });
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

export type BulkUpdateType = 'Upsert' | 'ChangeStatus' | 'Delete' | 'Validate';

export type ContentReferencesValue =
    Readonly<{ [partition: string]: string }> | string;

export type ContentReferences =
    Readonly<{ [fieldName: string ]: ContentFieldData<ContentReferencesValue> }>;

export type ContentFieldData<T = any> =
    Readonly<{ [partition: string]: T }>;

export type ContentData =
    Readonly<{ [fieldName: string ]: ContentFieldData }>;

export type BulkUpdateDto =
    Readonly<{ jobs: ReadonlyArray<BulkUpdateJobDto>; doNotScript?: boolean; checkReferrers?: boolean }>;

export type BulkUpdateJobDto =
    Readonly<{ id: string; type: BulkUpdateType; status?: string; schema?: string; dueTime?: string | null; expectedVersion?: number }>;

export type ContentsByIds =
    Readonly<{ ids: ReadonlyArray<string> }>;

export type ContentsBySchedule =
    Readonly<{ scheduledFrom: string | null; scheduledTo: string | null }>;

export type ContentsByQuery =
    Readonly<{ query?: Query; skip?: number; take?: number }>;

@Injectable()
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getContents(appName: string, schemaName: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const { odataParts, queryObj } = buildQuery(q);

        const body: any = {};

        if (odataParts.length > 0) {
            body.odataQuery = odataParts.join('&');
        } else if (queryObj) {
            body.q = queryObj;
        }

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/query`);

        return this.http.post<{ total: number; items: []; statuses: StatusInfo[] } & Resource>(url, body).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(parseContent);

                return new ContentsDto(statuses, total, contents, _links);
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
        const url = this.apiUrl.buildUrl(`/api/content/${appName}`);

        return this.http.post<{ total: number; items: []; statuses: StatusInfo[] } & Resource>(url, q).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(parseContent);

                return new ContentsDto(statuses, total, contents, _links);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferences(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const { fullQuery } = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/references?${fullQuery}`);

        return this.http.get<{ total: number; items: []; statuses: StatusInfo[] } & Resource>(url).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(parseContent);

                return new ContentsDto(statuses, total, contents, _links);
            }),
            pretifyError('i18n:contents.loadFailed'));
    }

    public getContentReferencing(appName: string, schemaName: string, id: string, q?: ContentsByQuery): Observable<ContentsDto> {
        const { fullQuery } = buildQuery(q);

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/referencing?${fullQuery}`);

        return this.http.get<{ total: number; items: []; statuses: StatusInfo[] } & Resource>(url).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(parseContent);

                return new ContentsDto(statuses, total, contents, _links);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'Created', appName);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'Updated', appName);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'Updated', appName);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'VersioNCreated', appName);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'Cancelled', appName);
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
            tap(() => {
                this.analytics.trackEvent('Content', 'VersionDeleted', appName);
            }),
            pretifyError('i18n:contents.deleteVersionFailed'));
    }

    public bulkUpdate(appName: string, schemaName: string, dto: BulkUpdateDto): Observable<ReadonlyArray<BulkResultDto>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/bulk`);

        return this.http.post<any[]>(url, dto).pipe(
            map(body => {
                return body.map(x => new BulkResultDto(x.contentId, parseError(x.error)));
            }),
            tap(() => {
                this.analytics.trackEvent('Content', 'Deleted', appName);
            }),
            pretifyError('i18n:contents.bulkFailed'));
    }
}

function buildQuery(q?: ContentsByQuery) {
    const { query, skip, take } = q || {};

    const queryParts: string[] = [];
    const odataParts: string[] = [];

    let queryObj: Query | undefined;

    if (query && query.fullText && query.fullText.indexOf('$') >= 0) {
        odataParts.push(`${query.fullText.trim()}`);

        if (take && take > 0) {
            odataParts.push(`$top=${take}`);
        }

        if (skip && skip > 0) {
            odataParts.push(`$skip=${skip}`);
        }
    } else {
        queryObj = { ...query };

        if (take && take > 0) {
            queryObj.take = take;
        }

        if (skip && skip > 0) {
            queryObj.skip = skip;
        }

        queryParts.push(`q=${encodeQuery(queryObj)}`);
    }

    const fullQuery = [...queryParts, ...odataParts].join('&');

    return { fullQuery, odataParts, queryObj };
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
