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
    HTTP,
    mapVersioned,
    pretifyError,
    Resource,
    ResourceLinks,
    Version,
    Versioned,
    withLinks
} from '@app/framework';

export type AppLanguagesDto = Versioned<AppLanguagesPayload>;
export type AppLanguagesPayload = { items: AppLanguageDto[] } & Resource;

export class AppLanguageDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
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
        const link = resource._links['update'];

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

function parseLanguages(body: any) {
    const items: any[] = body.items;

    const languages = items.map(item =>
        withLinks(
            new AppLanguageDto(
                item.iso2Code,
                item.englishName,
                item.isMaster,
                item.isOptional,
                item.fallback || []),
            item));

    return withLinks({ items: languages, _links: {} }, body);
}