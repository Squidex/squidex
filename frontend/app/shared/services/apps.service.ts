/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, ErrorDto, getLinkUrl, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, StringHelper, Types, Version, Versioned } from '@app/framework';
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

    public readonly displayName: string;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
        public readonly name: string,
        public readonly label: string | undefined,
        public readonly description: string | undefined,
        public readonly permissions: ReadonlyArray<string>,
        public readonly canAccessApi: boolean,
        public readonly canAccessContent: boolean,
        public readonly planName: string | undefined,
        public readonly planUpgrade: string | undefined,
        public readonly roleProperties: {},
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

        this.displayName = StringHelper.firstNonEmpty(this.label, this.name);
    }
}

export class AppSettingsDto {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly hideScheduler: boolean,
        public readonly patterns: ReadonlyArray<PatternDto>,
        public readonly editors: ReadonlyArray<EditorDto>,
        public readonly version: Version,
    ) {
        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class PatternDto {
    constructor(
        public readonly name: string,
        public readonly regex: string,
        public readonly message?: string,
    ) {
    }
}

export class EditorDto {
    constructor(
        public readonly name: string,
        public readonly url: string,
    ) {
    }
}

export type AssetScriptsDto =
    Versioned<AssetScriptsPayload>;

export type AssetScriptsPayload =
    Readonly<{ scripts: AssetScripts; canUpdate: boolean } & Resource>;

export type UpdateAppSettingsDto =
    Readonly<{ patterns: ReadonlyArray<PatternDto>; editors: ReadonlyArray<EditorDto>; hideScheduler?: boolean }>;

export type AssetScripts =
    Readonly<{ [name: string]: string | null }>;

export type CreateAppDto =
    Readonly<{ name: string; template?: string }>;

export type UpdateAppDto =
    Readonly<{ label?: string; description?: string }>;

@Injectable()
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getApps(): Observable<ReadonlyArray<AppDto>> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const apps = body.map(parseApp);

                return apps;
            }),
            pretifyError('i18n:apps.loadFailed'));
    }

    public getApp(appName: string): Observable<AppDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${appName}`);

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

    public putApp(appName: string, resource: Resource, dto: UpdateAppDto, version: Version): Observable<AppDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseApp(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('App', 'Updated', appName);
            }),
            pretifyError('i18n:apps.updateFailed'));
    }

    public getSettings(name: string): Observable<AppSettingsDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${name}/settings`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseAppSettings(body);
            }),
            pretifyError('i18n:apps.loadSettingsFailed'));
    }

    public putSettings(appName: string, resource: Resource, dto: UpdateAppSettingsDto, version: Version): Observable<AppSettingsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseAppSettings(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('App', 'Updated', appName);
            }),
            pretifyError('i18n:apps.updateSettingsFailed'));
    }

    public getAssetScripts(name: string): Observable<AssetScriptsDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${name}/assets/scripts`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseAssetScripts(body);
            }),
            pretifyError('i18n:apps.loadAssetScriptsFailed'));
    }

    public putAssetScripts(appName: string, resource: Resource, dto: AssetScripts, version: Version): Observable<AssetScriptsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseAssetScripts(body);
            }),
            tap(() => {
                this.analytics.trackEvent('App', 'Updated', appName);
            }),
            pretifyError('i18n:apps.updateAssetScriptsFailed'));
    }

    public postAppImage(appName: string, resource: Resource, file: File, version: Version): Observable<number | AppDto> {
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
                    throw new Error('Invalid');
                }
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(() => new ErrorDto(413, 'i18n:apps.uploadImageTooBig'));
                } else {
                    return throwError(() => error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('AppImage', 'Uploaded', appName);
                }
            }),
            pretifyError('i18n:apps.uploadImageFailed'));
    }

    public deleteAppImage(appName: string, resource: Resource, version: Version): Observable<any> {
        const link = resource._links['image/delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            map(({ payload }) => {
                return parseApp(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('AppImage', 'Removed', appName);
            }),
            pretifyError('i18n:apps.removeImageFailed'));
    }

    public leaveApp(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['leave'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('App', 'Left', appName);
            }),
            pretifyError('i18n:apps.leaveFailed'));
    }

    public deleteApp(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('App', 'Archived', appName);
            }),
            pretifyError('i18n:apps.archiveFailed'));
    }
}

function parseApp(response: any) {
    return new AppDto(response._links,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.name,
        response.label,
        response.description,
        response.permissions,
        response.canAccessApi,
        response.canAccessContent,
        response.planName,
        response.planUpgrade,
        response.roleProperties);
}

function parseAppSettings(response: any) {
    return new AppSettingsDto(response._links,
        response.hideScheduler,
        response.patterns.map((x: any) => {
            return new PatternDto(x.name, x.regex, x.message);
        }),
        response.editors.map((x: any) => {
            return new EditorDto(x.name, x.url);
        }),
        new Version(response.version.toString()));
}

function parseAssetScripts(response: any) {
    const { _links, ...scripts } = response;

    return { scripts, _links, canUpdate: hasAnyLink(_links, 'update') };
}
