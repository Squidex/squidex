/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class ContentsDto {
    constructor(
        public readonly total: number,
        public readonly items: ContentDto[]
    ) {
    }
}

export class ContentDto {
    constructor(
        public readonly id: string,
        public readonly status: string,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly scheduledTo: string | null,
        public readonly scheduledBy: string | null,
        public readonly scheduledAt: DateTime | null,
        public readonly data: any,
        public readonly version: Version
    ) {
    }

    public setData(data: any): ContentDto {
        return new ContentDto(
            this.id,
            this.status,
            this.createdBy,
            this.lastModifiedBy,
            this.created,
            this.lastModified,
            this.scheduledTo,
            this.scheduledBy,
            this.scheduledAt,
            data,
            this.version);
    }

    public changeStatus(status: string, dueTime: DateTime | null, user: string, version: Version, now?: DateTime): ContentDto {
        if (dueTime) {
            return new ContentDto(
                this.id,
                this.status,
                this.createdBy, user,
                this.created, now || DateTime.now(),
                status,
                user,
                dueTime,
                this.data,
                version);
        } else {
            return new ContentDto(
                this.id,
                status,
                this.createdBy, user,
                this.created, now || DateTime.now(),
                null,
                null,
                null,
                this.data,
                version);
        }
    }

    public update(data: any, user: string, version: Version, now?: DateTime): ContentDto {
        return new ContentDto(
            this.id,
            this.status,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.scheduledTo,
            this.scheduledBy,
            this.scheduledAt,
            data,
            version);
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

    public getContents(appName: string, schemaName: string, take: number, skip: number, query?: string, ids?: string[], archived = false): Observable<ContentsDto> {
        const queryParts: string[] = [];

        if (query && query.length > 0) {
            if (query.indexOf('$filter') < 0 &&
                query.indexOf('$search') < 0 &&
                query.indexOf('$orderby') < 0) {
                queryParts.push(`$search="${query.trim()}"`);
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

        if (archived) {
            queryParts.push('archived=true');
        }

        const fullQuery = queryParts.join('&');

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?${fullQuery}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.items;

                    return new ContentsDto(body.total, items.map(item => {
                        return new ContentDto(
                            item.id,
                            item.status,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            item.scheduledTo || null,
                            item.scheduledBy || null,
                            item.scheduledAt ? DateTime.parseISO_UTC(item.scheduledAt) : null,
                            item.data,
                            new Version(item.version.toString()));
                    }));
                })
                .pretifyError('Failed to load contents. Please reload.');
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    return new ContentDto(
                        body.id,
                        body.status,
                        body.createdBy,
                        body.lastModifiedBy,
                        DateTime.parseISO_UTC(body.created),
                        DateTime.parseISO_UTC(body.lastModified),
                        body.scheduledTo || null,
                        body.scheduledBy || null,
                        body.scheduledAt || null ? DateTime.parseISO_UTC(body.scheduledAt) : null,
                        body.data,
                        response.version);
                })
                .pretifyError('Failed to load content. Please reload.');
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version.value}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    return response.payload.body;
                })
                .pretifyError('Failed to load data. Please reload.');
    }

    public postContent(appName: string, schemaName: string, dto: any, publish: boolean): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?publish=${publish}`);

        return HTTP.postVersioned<any>(this.http, url, dto)
                .map(response => {
                    const body = response.payload.body;

                    return new ContentDto(
                        body.id,
                        body.status,
                        body.createdBy,
                        body.lastModifiedBy,
                        DateTime.parseISO_UTC(body.created),
                        DateTime.parseISO_UTC(body.lastModified),
                        null,
                        null,
                        null,
                        body.data,
                        response.version);
                })
                .do(content => {
                    this.analytics.trackEvent('Content', 'Created', appName);
                })
                .pretifyError('Failed to create content. Please reload.');
    }

    public putContent(appName: string, schemaName: string, id: string, dto: any, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .map(response => {
                    const body = response.payload.body;

                    return new Versioned(response.version, body);
                })
                .do(() => {
                    this.analytics.trackEvent('Content', 'Updated', appName);
                })
                .pretifyError('Failed to update content. Please reload.');
    }

    public patchContent(appName: string, schemaName: string, id: string, dto: any, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.patchVersioned(this.http, url, dto, version)
                .map(response => {
                    const body = response.payload.body;

                    return new Versioned(response.version, body);
                })
                .do(() => {
                    this.analytics.trackEvent('Content', 'Updated', appName);
                })
                .pretifyError('Failed to update content. Please reload.');
    }

    public deleteContent(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .do(() => {
                    this.analytics.trackEvent('Content', 'Deleted', appName);
                })
                .pretifyError('Failed to delete content. Please reload.');
    }

    public changeContentStatus(appName: string, schemaName: string, id: string, action: string, dueTime: string | null, version: Version): Observable<Versioned<any>> {
        let url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${action}`);

        if (dueTime) {
            url += `?dueTime=${dueTime}`;
        }

        return HTTP.putVersioned(this.http, url, {}, version)
                .do(() => {
                    this.analytics.trackEvent('Content', 'Archived', appName);
                })
                .pretifyError(`Failed to ${action} content. Please reload.`);
    }
}