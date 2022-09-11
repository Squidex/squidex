/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';

export class AppLanguageDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: ReadonlyArray<string>,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export type AppLanguagesDto = Versioned<AppLanguagesPayload>;

export type AppLanguagesPayload = Readonly<{
    // The app languages.
    items: ReadonlyArray<AppLanguageDto>;

    // The if the user can create a new language.
    canCreate?: boolean;
}>;

export type AddAppLanguageDto = Readonly<{
    // The language code to add.
    language: string;
}>;

export type UpdateAppLanguageDto = Readonly<{
    // Indicates if the language is the master language.
    isMaster?: boolean;

    // Indicates if the langauge is optional (cannot be master language).
    isOptional?: boolean;

    // The fallback language codes.
    falback?: ReadonlyArray<string>;
}>;

@Injectable()
export class AppLanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getLanguages(appName: string): Observable<AppLanguagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            pretifyError('i18n:languages.loadFailed'));
    }

    public postLanguage(appName: string, dto: AddAppLanguageDto, version: Version): Observable<AppLanguagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            pretifyError('i18n:languages.addFailed'));
    }

    public putLanguage(appName: string, resource: Resource, dto: UpdateAppLanguageDto, version: Version): Observable<AppLanguagesDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            pretifyError('i18n:languages.updateFailed'));
    }

    public deleteLanguage(appName: string, resource: Resource, version: Version): Observable<AppLanguagesDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseLanguages(body);
            }),
            pretifyError('i18n:languages.deleteFailed'));
    }
}

function parseLanguages(response: { items: any[] } & Resource): AppLanguagesPayload {
    const { items: list, _links } = response;
    const items = list.map(parseLanguage);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
}

function parseLanguage(response: any) {
    return new AppLanguageDto(response._links,
        response.iso2Code,
        response.englishName,
        response.isMaster,
        response.isOptional,
        response.fallback || []);
}
