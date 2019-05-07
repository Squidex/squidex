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
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

import { LanguageDto } from './languages.service';

export type AppLanguagesDto = Versioned<AppLanguageDto[]>;

export class AppLanguageDto extends Model<AppLanguageDto> {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
        super();
    }

    public static fromLanguage(language: LanguageDto, isMaster = false, isOptional = false, fallback: string[] = []) {
        return new AppLanguageDto(
            language.iso2Code,
            language.englishName,
            isMaster,
            isOptional,
            fallback);
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

        return HTTP.getVersioned<any>(this.http, url).pipe(
                mapVersioned(({ body }) => {
                    const items: any[] = body;

                    const languages = items.map(item =>
                        new AppLanguageDto(
                            item.iso2Code,
                            item.englishName,
                            item.isMaster,
                            item.isOptional,
                            item.fallback || []));

                    return languages;
                }),
                pretifyError('Failed to load languages. Please reload.'));
    }

    public postLanguage(appName: string, dto: AddAppLanguageDto, version: Version): Observable<Versioned<AppLanguageDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
                mapVersioned(({ body }) => {
                    const language = new AppLanguageDto(
                        body.iso2Code,
                        body.englishName,
                        body.isMaster,
                        body.isOptional,
                        body.fallback || []);

                    return language;
                }),
                tap(() => {
                    this.analytics.trackEvent('Language', 'Added', appName);
                }),
                pretifyError('Failed to add language. Please reload.'));
    }

    public putLanguage(appName: string, languageCode: string, dto: UpdateAppLanguageDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Language', 'Updated', appName);
                }),
                pretifyError('Failed to change language. Please reload.'));
    }

    public deleteLanguage(appName: string, languageCode: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Language', 'Deleted', appName);
                }),
                pretifyError('Failed to add language. Please reload.'));
    }
}