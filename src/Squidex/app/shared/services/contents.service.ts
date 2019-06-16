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

export class ContentsDto extends ResultSet<ContentDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }

    public get canCreateAndPublish() {
        return hasAnyLink(this._links, 'create/publish');
    }
}

export class ContentDto {
    public readonly _links: ResourceLinks;

    public readonly statusUpdates: string[];

    public readonly canDelete: boolean;
    public readonly canDiscardChanges: boolean;
    public readonly canProposeChange: boolean;
    public readonly canPublishChanges: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly status: string,
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

        this.statusUpdates = Object.keys(links).filter(x => x.startsWith('status/')).map(x => x.substr(7));
    }
}

export type ContentQueryStatus = 'Archived' | 'PublishedOnly' | 'PublishedDraft';

@Injectable()
export class ContentsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getContents(appName: string, schemaName: string, take: number, skip: number, query?: string, ids?: string[], status: ContentQueryStatus = 'PublishedDraft'): Observable<ContentsDto> {
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
            queryParts.push(`status=${status}`);
        }

        const fullQuery = queryParts.join('&');

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?${fullQuery}`);

        return this.http.get<{ total: number, items: [] } & Resource>(url).pipe(
                map(({ total, items, _links }) => {
                    const contents = items.map(x => parseContent(x));

                    return new ContentsDto(total, contents, _links);
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

    public putContent(appName: string, resource: Resource, dto: any, asDraft: boolean, version: Version): Observable<ContentDto> {
        const link = resource._links[asDraft ? 'update/change' : 'update'];

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

    public discardChanges(appName: string, resource: Resource, version: Version): Observable<ContentDto> {
        const link = resource._links['discard'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
                map(({ payload }) => {
                    return parseContent(payload.body);
                }),
                tap(() => {
                    this.analytics.trackEvent('Content', 'Discarded', appName);
                }),
                pretifyError('Failed to discard changes. Please reload.'));
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
        new Version(response.version));
}