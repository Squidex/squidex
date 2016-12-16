/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlConfig, handleError } from 'framework';

import { AuthService } from './auth.service';

export class AppLanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMasterLanguage: boolean
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
        public readonly isMasterLanguage: boolean
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

    public getLanguages(appName: string): Observable<AppLanguageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppLanguageDto(
                            item.iso2Code,
                            item.englishName,
                            item.isMasterLanguage === true);
                    });
                })
                .catch(response => handleError('Failed to load languages. Please reload', response));
    }

    public postLanguages(appName: string, dto: AddAppLanguageDto): Observable<AppLanguageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new AppLanguageDto(
                        response.iso2Code,
                        response.englishName,
                        response.isMasterLanguage === true);
                })
                .catch(response => handleError('Failed to add language. Please reload.', response));
    }

    public updateLanguage(appName: string, languageCode: string, dto: UpdateAppLanguageDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authPut(url, dto)
                .catch(response => handleError('Failed to change language. Please reload.', response));
    }

    public deleteLanguage(appName: string, languageCode: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authDelete(url)
                .catch(response => handleError('Failed to add language. Please reload.', response));
    }
}