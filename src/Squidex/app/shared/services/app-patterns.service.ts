/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class AppPatternsDto {
    constructor(
        public readonly patterns: AppPatternDto[],
        public readonly version: Version
    ) {
    }

    public addPattern(pattern: AppPatternDto, version: Version) {
        return new AppPatternsDto([...this.patterns, pattern],  version);
    }

    public updatePattern(pattern: AppPatternDto, version: Version) {
        return new AppPatternsDto(this.patterns.map(p => p.patternId === pattern.patternId ? pattern : p), version);
    }

    public deletePattern(pattern: AppPatternDto, version: Version) {
        return new AppPatternsDto(this.patterns.filter(c => c.patternId !== pattern.patternId), version);
    }
}

export class AppPatternDto {
    constructor(
        public readonly patternId: string,
        public readonly name: string,
        public readonly pattern: string,
        public readonly message: string
    ) {
    }

    public update(update: UpdatePatternDto) {
        return new AppPatternDto(
            this.patternId,
            update.name,
            update.pattern,
            update.message);
    }
}

export class UpdatePatternDto {
    constructor(
        public readonly name: string,
        public readonly pattern: string,
        public readonly message: string
    ) {
    }
}


@Injectable()
export class AppPatternsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPatterns(appName: string): Observable<AppPatternsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

                const items: any[] = body;

                return new AppPatternsDto(
                    items.map(item => {
                        return new AppPatternDto(
                            item.patternId,
                            item.name,
                            item.pattern,
                            item.message);
                    }),
                    response.version);
            })
            .pretifyError('Failed to add pattern. Please reload.');
    }

    public postPattern(appName: string, dto: UpdatePatternDto, version: Version): Observable<Versioned<AppPatternDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.postVersioned<any>(this.http, url, dto, version)
            .map(response => {
                const body = response.payload.body;

                const pattern = new AppPatternDto(
                    body.patternId,
                    body.name,
                    body.pattern,
                    body.message);

                return new Versioned(response.version, pattern);
            })
            .pretifyError('Failed to add pattern. Please reload.');
    }

    public putPattern(appName: string, id: string, dto: UpdatePatternDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .pretifyError('Failed to update pattern. Please reload.');
    }

    public deletePattern(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .pretifyError('Failed to remove pattern. Please reload.');
    }
}