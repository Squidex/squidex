/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, ResultSet, Version, Versioned } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { encodeQuery, Query } from './../state/query';
import { parseField, RootFieldDto } from './schemas.service';

export class ScheduleDto {
    constructor(
        public readonly status: string,
        public readonly scheduledBy: string,
        public readonly color: string,
        public readonly dueTime: DateTime
    ) {
    }
}

export type StatusInfo = { status: string; color: string; };

export class ContentsDto extends ResultSet<ContentDto> {
    constructor(
        public readonly statuses: ReadonlyArray<StatusInfo>,
        total: number,
        items: ReadonlyArray<ContentDto>,
        links?: ResourceLinks
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

export type ContentReferencesValue = { [partition: string]: string } | string;
export type ContentReferences = { [fieldName: string ]: ContentFieldData<ContentReferencesValue> };
export type ContentFieldData<T = any> = { [partition: string]: T };
export type ContentData = { [fieldName: string ]: ContentFieldData };

export class ContentDto {
    public readonly _links: ResourceLinks;

    public readonly statusUpdates: ReadonlyArray<StatusInfo>;

    public readonly canDelete: boolean;
    public readonly canDraftDelete: boolean;
    public readonly canDraftCreate: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly status: string,
        public readonly statusColor: string,
        public readonly newStatus: string | undefined,
        public readonly newStatusColor: string | undefined,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly scheduleJob: ScheduleDto | null,
        public readonly data: ContentData,
        public readonly schemaName: string,
        public readonly schemaDisplayName: string,
        public readonly referenceData: ContentReferences,
        public readonly referenceFields: ReadonlyArray<RootFieldDto>,
        public readonly version: Version
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDraftCreate = hasAnyLink(links, 'draft/create');
        this.canDraftDelete = hasAnyLink(links, 'draft/delete');
        this.canUpdate = hasAnyLink(links, 'update');

        this.statusUpdates = Object.keys(links).filter(x => x.startsWith('status/')).map(x => ({ status: x.substr(7), color: links[x].metadata! }));
    }
}

export interface ContentQueryDto {
    readonly ids?: ReadonlyArray<string>;
    readonly maxLength?: number;
    readonly query?: Query;
    readonly skip?: number;
    readonly take?: number;
}

@Injectable()
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getContents(appName: string, schemaName: string, q?: ContentQueryDto): Observable<ContentsDto> {
        const { ids, maxLength, query, skip, take } = q || {};

        const queryParts: string[] = [];
        const queryOdataParts: string[] = [];

        let queryObj: Query | undefined;

        if (ids && ids.length > 0) {
            queryParts.push(`ids=${ids.join(',')}`);
        } else {

            if (query && query.fullText && query.fullText.indexOf('$') >= 0) {
                queryOdataParts.push(`${query.fullText.trim()}`);

                if (take && take > 0) {
                    queryOdataParts.push(`$top=${take}`);
                }

                if (skip && skip > 0) {
                    queryOdataParts.push(`$skip=${skip}`);
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
        }

        const fullQuery = [...queryParts, ...queryOdataParts].join('&');

        if (fullQuery.length > (maxLength || 2000)) {
            const body: any = {};

            if (ids && ids.length > 0) {
                body.ids = ids;
            } else {
                if (queryOdataParts.length > 0) {
                    body.odataQuery = queryOdataParts.join('&');
                } else if (queryObj) {
                    body.q = queryObj;
                }
            }

            const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/query`);

            return this.http.post<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url, body).pipe(
                map(({ total, items, statuses, _links }) => {
                    const contents = items.map(x => parseContent(x));

                    return new ContentsDto(statuses, total, contents, _links);
                }),
                pretifyError('i18n:contents.loadFailed'));
        } else {
            const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?${fullQuery}`);

            return this.http.get<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url).pipe(
                map(({ total, items, statuses, _links }) => {
                    const contents = items.map(x => parseContent(x));

                    return new ContentsDto(statuses, total, contents, _links);
                }),
                pretifyError('i18n:contents.loadFailed'));
        }
    }

    public getContentsByIds(appName: string, ids: ReadonlyArray<string>, maxLength?: number): Observable<ContentsDto> {
        const fullQuery = `ids=${ids.join(',')}`;

        if (fullQuery.length > (maxLength || 2000)) {
            const body = { ids };

            const url = this.apiUrl.buildUrl(`/api/content/${appName}`);

            return this.http.post<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url, body).pipe(
                map(({ total, items, statuses, _links }) => {
                    const contents = items.map(x => parseContent(x));

                    return new ContentsDto(statuses, total, contents, _links);
                }),
                pretifyError('i18n:contents.loadFailed'));

        } else {
            const url = this.apiUrl.buildUrl(`/api/content/${appName}?${fullQuery}`);

            return this.http.get<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url).pipe(
                map(({ total, items, statuses, _links }) => {
                    const contents = items.map(x => parseContent(x));

                    return new ContentsDto(statuses, total, contents, _links);
                }),
                pretifyError('i18n:contents.loadFailed'));
        }
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('i18n:contents.loadContentFailed'));
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version.value}`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return body;
            }),
            pretifyError('i18n:contents.loadDataFailed'));
    }

    public postContent(appName: string, schemaName: string, dto: any, publish: boolean): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?publish=${publish}`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
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

    public putStatus(appName: string, resource: Resource, status: string, dueTime: string | null, version: Version): Observable<ContentDto> {
        const link = resource._links[`status/${status}`];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { status, dueTime }).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Content', 'Archived', appName);
            }),
            pretifyError(`Failed to ${status} content. Please reload.`));
    }

    public deleteContent(appName: string, resource: Resource, checkReferrers: boolean, version: Version): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href) + `?checkReferrers=${checkReferrers}`;

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Content', 'Deleted', appName);
            }),
            pretifyError('i18n:contents.deleteFailed'));
    }
}

function parseContent(response: any) {
    return new ContentDto(response._links,
        response.id,
        response.status,
        response.statusColor,
        response.newStatus,
        response.newStatusColor,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        parseScheduleJob(response.scheduleJob),
        response.data,
        response.schemaName,
        response.schemaDisplayName,
        response.referenceData,
        response.referenceFields.map((item: any) => parseField(item)),
        new Version(response.version.toString()));
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