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

export class ScheduleDto {
    constructor(
        public readonly status: string,
        public readonly scheduledBy: string,
        public readonly dueTime: DateTime
    ) {
    }
}

export type StatusInfo = { status: string; color: string; };

export class ContentsDto extends ResultSet<ContentDto> {
    constructor(
        public readonly statuses: StatusInfo[],
        total: number,
        items: ContentDto[],
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

export class ContentDto {
    public readonly _links: ResourceLinks;

    public readonly statusUpdates: StatusInfo[];

    public readonly canDelete: boolean;
    public readonly canDraftDiscard: boolean;
    public readonly canDraftPropose: boolean;
    public readonly canDraftPublish: boolean;
    public readonly canUpdate: boolean;
    public readonly canUpdateAny: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly status: string,
        public readonly statusColor: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly scheduleJob: ScheduleDto | null,
        public readonly isPending: boolean,
        public readonly data: object | any,
        public readonly dataDraft: object,
        public readonly version: Version
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDraftDiscard = hasAnyLink(links, 'draft/discard');
        this.canDraftPropose = hasAnyLink(links, 'draft/propose');
        this.canDraftPublish = hasAnyLink(links, 'draft/publish');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpdateAny = this.canUpdate || this.canDraftPropose;

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

    public getContents(appName: string, schemaName: string, take: number, skip: number, query?: string, ids?: string[], status?: string[]): Observable<ContentsDto> {
        const queryParts: string[] = [];

        if (query && query.length > 0) {
            if (query.indexOf('$filter') < 0 &&
                query.indexOf('$search') < 0 &&
                query.indexOf('$orderby') < 0) {
                queryParts.push(`$search="${encodeURIComponent(query.trim())}"`);
            } else {
                queryParts.push(`${query.trim()}`);
            }
        }

        if (take > 0) {
            queryParts.push(`$top=${take}`);
        }

        if (skip > 0) {
            queryParts.push(`$skip=${skip}`);
        }

        if (ids && ids.length > 0) {
            queryParts.push(`ids=${ids.join(',')}`);
        }

        if (status) {
            for (let s of status) {
                queryParts.push(`status=${s}`);
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

    public discardDraft(appName: string, resource: Resource, version: Version): Observable<ContentDto> {
        const link = resource._links['draft/discard'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Content', 'Discarded', appName);
            }),
            pretifyError('Failed to discard draft. Please reload.'));
    }

    public proposeDraft(appName: string, resource: Resource, dto: any, version: Version): Observable<ContentDto> {
        const link = resource._links['draft/propose'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Content', 'Updated', appName);
            }),
            pretifyError('Failed to propose draft. Please reload.'));
    }

    public publishDraft(appName: string, resource: Resource, dueTime: string | null, version: Version): Observable<ContentDto> {
        const link = resource._links['draft/publish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { status: 'Published', dueTime }).pipe(
            map(({ payload }) => {
                return parseContent(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Content', 'Discarded', appName);
            }),
            pretifyError('Failed to publish draft. Please reload.'));
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
        DateTime.parseISO_UTC(response.created), response.createdBy,
        DateTime.parseISO_UTC(response.lastModified), response.lastModifiedBy,
        response.scheduleJob
            ? new ScheduleDto(
                response.scheduleJob.status,
                response.scheduleJob.scheduledBy,
                DateTime.parseISO_UTC(response.scheduleJob.dueTime))
            : null,
        response.isPending === true,
        response.data,
        response.dataDraft,
        new Version(response.version.toString()));
}