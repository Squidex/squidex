/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, Version } from 'framework';
import { AuthService } from './auth.service';

export class AppLanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
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
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(appName: string, version?: Version): Observable<AppLanguageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
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
                .catchError('Failed to load languages. Please reload.');
    }

    public postLanguages(appName: string, dto: AddAppLanguageDto, version?: Version): Observable<AppLanguageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authPost(url, dto, version)
                .map(response => response.json())
                .map(response => {
                    return new AppLanguageDto(
                        response.iso2Code,
                        response.englishName,
                        response.isMaster === true,
                        response.isOptional === true,
                        response.fallback || []);
                })
                .catchError('Failed to add language. Please reload.');
    }

    public updateLanguage(appName: string, languageCode: string, dto: UpdateAppLanguageDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authPut(url, dto, version)
                .catchError('Failed to change language. Please reload.');
    }

    public deleteLanguage(appName: string, languageCode: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authDelete(url, version)
                .catchError('Failed to add language. Please reload.');
    }
}