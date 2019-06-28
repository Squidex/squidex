/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    hasAnyLink,
    HTTP,
    mapVersioned,
    pretifyError,
    Resource,
    ResourceLinks,
    Version,
    Versioned
} from '@app/framework';

export type AppLanguagesDto = Versioned<AppLanguagesPayload>;
export type AppLanguagesPayload = {
    readonly items: AppLanguageDto[];

    readonly canCreate: boolean;
} & Resource;

export class AppLanguageDto {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;
    public readonly canDelete: boolean;

    constructor(
        links: ResourceLinks,
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
        this.canDelete = hasAnyLink(links, 'delete');
    }
}

export interface AddAppLanguageDto {
    readonly language: string;
}

export interface UpdateAppLanguageDto {
    readonly isMaster?: boolean;
    readonly isOptional?: boolean;
    readonly fallback?: string[];
}

@Injectable()
export class AppLanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getLanguages(appName: string): Observable<AppLanguagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            pretifyError('Failed to load languages. Please reload.'));
    }

    public postLanguage(appName: string, dto: AddAppLanguageDto, version: Version): Observable<AppLanguagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Language', 'Added', appName);
            }),
            pretifyError('Failed to add language. Please reload.'));
    }

    public putLanguage(appName: string, resource: Resource, dto: UpdateAppLanguageDto, version: Version): Observable<AppLanguagesDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Language', 'Updated', appName);
            }),
            pretifyError('Failed to change language. Please reload.'));
    }

    public deleteLanguage(appName: string, resource: Resource, version: Version): Observable<AppLanguagesDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Language', 'Deleted', appName);
            }),
            pretifyError('Failed to add language. Please reload.'));
    }
}

function parseLanguages(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new AppLanguageDto(item._links,
            item.iso2Code,
            item.englishName,
            item.isMaster,
            item.isOptional,
            item.fallback || []));

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}