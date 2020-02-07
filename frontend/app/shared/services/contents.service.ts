/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    hasAnyLink,
    HTTP,
    mapVersioned,
    pretifyError,
    Resource,
    ResourceLinks,
    ResultSet,
    Version,
    Versioned
} from '@app/framework';

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

@Injectable()
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getContents(appName: string, schemaName: string, take: number, skip: number, query?: Query, ids?: ReadonlyArray<string>): Observable<ContentsDto> {
        const queryParts: string[] = [];

        if (ids && ids.length > 0) {
            queryParts.push(`ids=${ids.join(',')}`);
        } else {
            const queryObj: Query = { ...query };

            if (queryObj.fullText && queryObj.fullText.indexOf('$') >= 0) {
                queryParts.push(`${queryObj.fullText.trim()}`);

                if (take > 0) {
                    queryParts.push(`$top=${take}`);
                }

                if (skip > 0) {
                    queryParts.push(`$skip=${skip}`);
                }
            } else {
                if (take > 0) {
                    queryObj.take = take;
                }

                if (skip > 0) {
                    queryObj.skip = skip;
                }

                queryParts.push(`q=${encodeQuery(queryObj)}`);
            }
        }

        const fullQuery = queryParts.join('&');

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?${fullQuery}`);

        return this.http.get<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(x => parseContent(x));

                return new ContentsDto(statuses, total, contents, _links);
            }),
            pretifyError('Failed to load contents. Please reload.'));
    }

    public getContentsByIds(appName: string, ids: ReadonlyArray<string>): Observable<ContentsDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/?ids=${ids.join(',')}`);

        return this.http.get<{ total: number, items: [], statuses: StatusInfo[] } & Resource>(url).pipe(
            map(({ total, items, statuses, _links }) => {
                const contents = items.map(x => parseContent(x));

                return new ContentsDto(statuses, total, contents, _links);
            }),
            pretifyError('Failed to load contents. Please reload.'));
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            pretifyError('Failed to load content. Please reload.'));
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version.value}`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return body;
            }),
            pretifyError('Failed to load data. Please reload.'));
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
            pretifyError('Failed to create content. Please reload.'));
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
            pretifyError('Failed to update content. Please reload.'));
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
            pretifyError('Failed to update content. Please reload.'));
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
            pretifyError('Failed to version a new version. Please reload.'));
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
            pretifyError('Failed to delete version. Please reload.'));
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

    public deleteContent(appName: string, resource: Resource, version: Version): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Content', 'Deleted', appName);
            }),
            pretifyError('Failed to delete content. Please reload.'));
    }
}

function parseContent(response: any) {
    return new ContentDto(response._links,
        response.id,
        response.status,
        response.statusColor,
        response.newStatus,
        response.newStatusColor,
        DateTime.parseISO_UTC(response.created), response.createdBy,
        DateTime.parseISO_UTC(response.lastModified), response.lastModifiedBy,
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
        DateTime.parseISO_UTC(response.dueTime));
}