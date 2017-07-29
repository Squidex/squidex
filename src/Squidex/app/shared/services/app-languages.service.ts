/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP,
    Version
} from 'framework';

export class AppLanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
    }

    public update(isMaster: boolean, isOptional: boolean, fallback: string[]): AppLanguageDto {
        return new AppLanguageDto(this.iso2Code, this.englishName, isMaster, isOptional, fallback);
    }
}

export class AddAppLanguageDto {
    constructor(
        public readonly language: string
    ) {
    }
}

export class UpdateAppLanguageDto {
    constructor(
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
    }
}

@Injectable()
export class AppLanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(appName: string, version?: Version): Observable<AppLanguageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.getVersioned(this.http, url, version)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppLanguageDto(
                            item.iso2Code,
                            item.englishName,
                            item.isMaster === true,
                            item.isOptional === true,
                            item.fallback || []);
                    });
                })
                .pretifyError('Failed to load languages. Please reload.');
    }

    public postLanguages(appName: string, dto: AddAppLanguageDto, version?: Version): Observable<AppLanguageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .map(response => {
                    return new AppLanguageDto(
                        response.iso2Code,
                        response.englishName,
                        response.isMaster === true,
                        response.isOptional === true,
                        response.fallback || []);
                })
                .pretifyError('Failed to add language. Please reload.');
    }

    public updateLanguage(appName: string, languageCode: string, dto: UpdateAppLanguageDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .pretifyError('Failed to change language. Please reload.');
    }

    public deleteLanguage(appName: string, languageCode: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to add language. Please reload.');
    }
}