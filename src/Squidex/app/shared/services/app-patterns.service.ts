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

export class AppPatternsSuggestionDto {
    public name: string;
    public pattern: string;
    public defaultMessage: string;

    constructor(name: string, pattern: string, message: string) {
        this.name = name;
        this.pattern = pattern;
        this.defaultMessage = message;
    }
}

@Injectable()
export class AppPatternsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPatterns(appName: string): Observable<AppPatternsSuggestionDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return this.http.get<AppPatternsSuggestionDto[]>(url)
            .catch(error => {
                return Observable.of({ AppPatternsSuggestionDto: [] });
            })
            .map((response: AppPatternsSuggestionDto[]) => response);
    }

    public postPattern(appName: string, dto: AppPatternsSuggestionDto, version: Version): Observable<AppPatternsSuggestionDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.postVersioned<any>(this.http, url, dto, version)
            .map(response => {
                const body = response.payload.body;
                return new AppPatternsSuggestionDto(
                    body.name || body.id,
                    body.pattern,
                    body.defaultMessage);
            })
            .pretifyError('Failed to add pattern. Please reload.');
    }

    public updatePattern(appName: string, name: string, dto: AppPatternsSuggestionDto, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${name}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .pretifyError('Failed to update pattern. Please reload.');
    }

    public deletePattern(appName: string, name: string, version: Version): Observable<AppPatternsSuggestionDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${name}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .pretifyError('Failed to remove pattern. Please reload.');
    }
}