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
    HTTP,
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export class AppLanguagesDto extends Model {
    constructor(
        public readonly languages: AppLanguageDto[],
        public readonly version: Version
    ) {
        super();
    }
}

export class AppLanguageDto extends Model {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMaster: boolean,
        public readonly isOptional: boolean,
        public readonly fallback: string[]
    ) {
        super();
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

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
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
                }),
                pretifyError('Failed to load languages. Please reload.'));
    }

    public postLanguage(appName: string, dto: AddAppLanguageDto, version: Version): Observable<Versioned<AppLanguageDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
                map(response => {
                    const body = response.payload.body;

                    const language = new AppLanguageDto(
                        body.iso2Code,
                        body.englishName,
                        body.isMaster === true,
                        body.isOptional === true,
                        body.fallback || []);

                    return new Versioned(response.version, language);
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