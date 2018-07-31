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
    HTTP,
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export class ContentsDto extends Model {
    constructor(
        public readonly total: number,
        public readonly items: ContentDto[]
    ) {

        super();
    }

    public with(value: Partial<ContentsDto>): ContentsDto {
        return this.clone(value);
    }}


export class ScheduleDto extends Model {
    constructor(
        public readonly status: string,
        public readonly scheduledBy: string,
        public readonly dueTime: DateTime
    ) {
        super();
    }
}

export class ContentDto extends Model {
    constructor(
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
        super();
    }

    public with(value: Partial<ContentDto>): ContentDto {
        return this.clone(value);
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

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.items;

                    return new ContentsDto(body.total, items.map(item => {
                        return new ContentDto(
                            item.id,
                            item.status,
                            DateTime.parseISO_UTC(item.created), item.createdBy,
                            DateTime.parseISO_UTC(item.lastModified), item.lastModifiedBy,
                            item.scheduleJob
                                ? new ScheduleDto(
                                    item.scheduleJob.status,
                                    item.scheduleJob.scheduledBy,
                                    DateTime.parseISO_UTC(item.scheduleJob.dueTime))
                                : null,
                            item.isPending === true,
                            item.data,
                            item.dataDraft,
                            new Version(item.version.toString()));
                    }));
                }),
                pretifyError('Failed to load contents. Please reload.'));
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    return new ContentDto(
                        body.id,
                        body.status,
                        DateTime.parseISO_UTC(body.created), body.createdBy,
                        DateTime.parseISO_UTC(body.lastModified), body.lastModifiedBy,
                        body.scheduleJob
                            ? new ScheduleDto(
                                body.scheduleJob.status,
                                body.scheduleJob.scheduledBy,
                                DateTime.parseISO_UTC(body.scheduleJob.dueTime))
                            : null,
                        body.isPending === true,
                        body.data,
                        body.dataDraft,
                        response.version);
                }),
                pretifyError('Failed to load content. Please reload.'));
    }

    public getVersionData(appName: string, schemaName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${version.value}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    return response.payload.body;
                }),
                pretifyError('Failed to load data. Please reload.'));
    }

    public postContent(appName: string, schemaName: string, dto: any, publish: boolean): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?publish=${publish}`);

        return HTTP.postVersioned<any>(this.http, url, dto).pipe(
                map(response => {
                    const body = response.payload.body;

                    return new ContentDto(
                        body.id,
                        body.status,
                        DateTime.parseISO_UTC(body.created), body.createdBy,
                        DateTime.parseISO_UTC(body.lastModified), body.lastModifiedBy,
                        null,
                        true,
                        null,
                        body.data,
                        response.version);
                }),
                tap(content => {
                    this.analytics.trackEvent('Content', 'Created', appName);
                }),
                pretifyError('Failed to create content. Please reload.'));
    }

    public putContent(appName: string, schemaName: string, id: string, dto: any, asDraft: boolean, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}?asDraft=${asDraft}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                map(response => {
                    const body = response.payload.body;

                    return new Versioned(response.version, body);
                }),
                tap(() => {
                    this.analytics.trackEvent('Content', 'Updated', appName);
                }),
                pretifyError('Failed to update content. Please reload.'));
    }

    public patchContent(appName: string, schemaName: string, id: string, dto: any, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.patchVersioned(this.http, url, dto, version).pipe(
                map(response => {
                    const body = response.payload.body;

                    return new Versioned(response.version, body);
                }),
                tap(() => {
                    this.analytics.trackEvent('Content', 'Updated', appName);
                }),
                pretifyError('Failed to update content. Please reload.'));
    }

    public discardChanges(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/discard`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Content', 'Discarded', appName);
                }),
                pretifyError('Failed to discard changes. Please reload.'));
    }

    public deleteContent(appName: string, schemaName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Content', 'Deleted', appName);
                }),
                pretifyError('Failed to delete content. Please reload.'));
    }

    public changeContentStatus(appName: string, schemaName: string, id: string, action: string, dueTime: string | null, version: Version): Observable<Versioned<any>> {
        let url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/${action}`);

        if (dueTime) {
            url += `?dueTime=${dueTime}`;
        }

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Content', 'Archived', appName);
                }),
                pretifyError(`Failed to ${action} content. Please reload.`));
    }
}