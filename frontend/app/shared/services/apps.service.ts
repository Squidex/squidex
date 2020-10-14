/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, ErrorDto, getLinkUrl, hasAnyLink, HTTP, pretifyError, Resource, ResourceLinks, StringHelper, Types, Version } from '@app/framework';
import { Observable, throwError } from 'rxjs';
import { catchError, filter, map, tap } from 'rxjs/operators';

export class AppDto {
    public readonly _links: ResourceLinks;

    public readonly canCreateSchema: boolean;
    public readonly canDelete: boolean;
    public readonly canReadAssets: boolean;
    public readonly canReadBackups: boolean;
    public readonly canReadClients: boolean;
    public readonly canReadContributors: boolean;
    public readonly canReadLanguages: boolean;
    public readonly canReadPatterns: boolean;
    public readonly canReadPlans: boolean;
    public readonly canReadRoles: boolean;
    public readonly canReadRules: boolean;
    public readonly canReadSchemas: boolean;
    public readonly canReadWorkflows: boolean;
    public readonly canUpdateGeneral: boolean;
    public readonly canUpdateImage: boolean;
    public readonly canUploadAssets: boolean;
    public readonly image: string;

    public get displayName() {
        return StringHelper.firstNonEmpty(this.label, this.name);
    }

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly name: string,
        public readonly label: string | undefined,
        public readonly description: string | undefined,
        public readonly permissions: ReadonlyArray<string>,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly canAccessApi: boolean,
        public readonly canAccessContent: boolean,
        public readonly planName: string | undefined,
        public readonly planUpgrade: string | undefined,
        public readonly roleProperties: {},
        public readonly version: Version
    ) {
        this._links = links;

        this.canCreateSchema = hasAnyLink(links, 'schemas/create');
        this.canDelete = hasAnyLink(links, 'delete');
        this.canReadAssets = hasAnyLink(links, 'assets');
        this.canReadBackups = hasAnyLink(links, 'backups');
        this.canReadClients = hasAnyLink(links, 'clients');
        this.canReadContributors = hasAnyLink(links, 'contributors');
        this.canReadLanguages = hasAnyLink(links, 'languages');
        this.canReadPatterns = hasAnyLink(links, 'patterns');
        this.canReadPlans = hasAnyLink(links, 'plans');
        this.canReadRoles = hasAnyLink(links, 'roles');
        this.canReadRules = hasAnyLink(links, 'rules');
        this.canReadSchemas = hasAnyLink(links, 'schemas');
        this.canReadWorkflows = hasAnyLink(links, 'workflows');
        this.canUpdateGeneral = hasAnyLink(links, 'update');
        this.canUpdateImage = hasAnyLink(links, 'image/upload');
        this.canUploadAssets = hasAnyLink(links, 'assets/create');

        this.image = getLinkUrl(links, 'image');
    }
}

export interface CreateAppDto {
    readonly name: string;
    readonly template?: string;
}

export interface UpdateAppDto {
    readonly label?: string;
    readonly description?: string;
}

@Injectable()
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getApps(): Observable<ReadonlyArray<AppDto>> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const apps = body.map(item => parseApp(item));

                return apps;
            }),
            pretifyError('i18n:apps.loadFailed'));
    }

    public getApp(name: string): Observable<AppDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${name}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const app = parseApp(body);

                return app;
            }),
            pretifyError('i18n:apps.appLoadFailed'));
    }

    public postApp(dto: CreateAppDto): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return this.http.post(url, dto).pipe(
            map(body => {
                return parseApp(body);
            }),
            tap(() => {
                this.analytics.trackEvent('App', 'Created', dto.name);
            }),
            pretifyError('i18n:apps.createFailed'));
    }

    public putApp(resource: Resource, dto: UpdateAppDto, version: Version): Observable<AppDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseApp(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('App', 'Updated');
            }),
            pretifyError('i18n:apps.updateFailed'));
    }

    public postAppImage(resource: Resource, file: File, version: Version): Observable<number | AppDto> {
        const link = resource._links['image/upload'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.upload(this.http, link.method, url, file, version).pipe(
            filter(event =>
                event.type === HttpEventType.UploadProgress ||
                event.type === HttpEventType.Response),
            map(event => {
                if (event.type === HttpEventType.UploadProgress) {
                    const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                    return percentDone;
                } else if (Types.is(event, HttpResponse)) {
                    return parseApp(event.body);
                } else {
                    throw 'Invalid';
                }
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(new ErrorDto(413, 'i18n:apps.uploadImageTooBig'));
                } else {
                    return throwError(error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('AppImage', 'Uploaded');
                }
            }),
            pretifyError('i18n:apps.uploadImageFailed'));
    }

    public deleteAppImage(resource: Resource, version: Version): Observable<any> {
        const link = resource._links['image/delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            map(({ payload }) => {
                return parseApp(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('AppImage', 'Removed');
            }),
            pretifyError('i18n:apps.removeImageFailed'));
    }

    public leaveApp(resource: Resource): Observable<any> {
        const link = resource._links['leave'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('App', 'Left');
            }),
            pretifyError('i18n:apps.leaveFailed'));
    }

    public deleteApp(resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('App', 'Archived');
            }),
            pretifyError('i18n:apps.archiveFailed'));
    }
}

function parseApp(response: any) {
    return new AppDto(response._links,
        response.id,
        response.name,
        response.label,
        response.description,
        response.permissions,
        DateTime.parseISO(response.created),
        DateTime.parseISO(response.lastModified),
        response.canAccessApi,
        response.canAccessContent,
        response.planName,
        response.planUpgrade,
        response.roleProperties,
        new Version(response.version.toString()));
}
