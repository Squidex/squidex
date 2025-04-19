/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { AddLanguageDto, AppLanguagesDto, UpdateLanguageDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class AppLanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getLanguages(appName: string): Observable<Versioned<AppLanguagesDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return AppLanguagesDto.fromJSON(body);
            }),
            pretifyError('i18n:languages.loadFailed'));
    }

    public postLanguage(appName: string, dto: AddLanguageDto, version: VersionOrTag): Observable<Versioned<AppLanguagesDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return AppLanguagesDto.fromJSON(body);
            }),
            pretifyError('i18n:languages.addFailed'));
    }

    public putLanguage(appName: string, resource: Resource, dto: UpdateLanguageDto, version: VersionOrTag): Observable<Versioned<AppLanguagesDto>> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            mapVersioned(({ body }) => {
                return AppLanguagesDto.fromJSON(body);
            }),
            pretifyError('i18n:languages.updateFailed'));
    }

    public deleteLanguage(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<AppLanguagesDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return AppLanguagesDto.fromJSON(body);
            }),
            pretifyError('i18n:languages.deleteFailed'));
    }
}
