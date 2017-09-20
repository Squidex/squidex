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
    AnalyticsService,
    ApiUrlConfig,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class AppLanguagesDto {
    constructor(
        public readonly languages: AppLanguageDto[],
        public readonly version: Version
    ) {
    }

    public addLanguage(language: AppLanguageDto, version: Version) {
        return new AppLanguagesDto([...this.languages, language], version);
    }

    public updateLanguage(language: AppLanguageDto, version: Version) {
        return new AppLanguagesDto(this.languages.map(l => l.iso2Code === language.iso2Code ? language : l), version);
    }

    public removeLanguage(language: AppLanguageDto, version: Version) {
        return new AppLanguagesDto(this.languages.filter(l => l.iso2Code !== language.iso2Code), version);
    }
}

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
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getLanguages(appName: string): Observable<AppLanguagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body;

                    const languages = items.map(item => {
                        return new AppLanguageDto(
                            item.iso2Code,
                            item.englishName,
                            item.isMaster === true,
                            item.isOptional === true,
                            item.fallback || []);
                    });

                    return new AppLanguagesDto(languages, response.version);
                })
                .pretifyError('Failed to load languages. Please reload.');
    }

    public postLanguages(appName: string, dto: AddAppLanguageDto, version: Version): Observable<Versioned<AppLanguageDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned<any>(this.http, url, dto, version)
                .map(response => {
                    const body = response.payload.body;

                    const language = new AppLanguageDto(
                        body.iso2Code,
                        body.englishName,
                        body.isMaster === true,
                        body.isOptional === true,
                        body.fallback || []);

                    return new Versioned(response.version, language);
                })
                .do(() => {
                    this.analytics.trackEvent('Language', 'Added', appName);
                })
                .pretifyError('Failed to add language. Please reload.');
    }

    public updateLanguage(appName: string, languageCode: string, dto: UpdateAppLanguageDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .do(() => {
                    this.analytics.trackEvent('Language', 'Updated', appName);
                })
                .pretifyError('Failed to change language. Please reload.');
    }

    public deleteLanguage(appName: string, languageCode: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .do(() => {
                    this.analytics.trackEvent('Language', 'Deleted', appName);
                })
                .pretifyError('Failed to add language. Please reload.');
    }
}