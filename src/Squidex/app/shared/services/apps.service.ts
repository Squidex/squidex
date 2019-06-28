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
    pretifyError,
    Resource,
    ResourceLinks
} from '@app/framework';

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
    public readonly canUploadAssets: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly name: string,
        public readonly permissions: string[],
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly canAccessApi: boolean,
        public readonly canAccessContent: boolean,
        public readonly planName?: string,
        public readonly planUpgrade?: string
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
        this.canUploadAssets = hasAnyLink(links, 'assets/create');
    }
}

export interface CreateAppDto {
    readonly name: string;
    readonly template?: string;
}

@Injectable()
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getApps(): Observable<AppDto[]> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const apps = body.map(item => parseApp(item));

                return apps;
            }),
            pretifyError('Failed to load apps. Please reload.'));
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
            pretifyError('Failed to create app. Please reload.'));
    }

    public deleteApp(resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('App', 'Archived');
            }),
            pretifyError('Failed to archive app. Please reload.'));
    }
}

function parseApp(response: any) {
    return new AppDto(response._links,
        response.id,
        response.name,
        response.permissions,
        DateTime.parseISO_UTC(response.created),
        DateTime.parseISO_UTC(response.lastModified),
        response.canAccessApi,
        response.canAccessContent,
        response.planName,
        response.planUpgrade);
}
