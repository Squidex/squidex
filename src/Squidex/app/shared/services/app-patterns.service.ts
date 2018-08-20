/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    ApiUrlConfig,
    HTTP,
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export class AppPatternsDto extends Model {
    constructor(
        public readonly patterns: AppPatternDto[],
        public readonly version: Version
    ) {
        super();
    }
}

export class AppPatternDto extends Model {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly pattern: string,
        public readonly message: string
    ) {
        super();
    }
}

export class EditAppPatternDto {
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

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(response => {
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
            }),
            pretifyError('Failed to add pattern. Please reload.'));
    }

    public postPattern(appName: string, dto: EditAppPatternDto, version: Version): Observable<Versioned<AppPatternDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
            map(response => {
                const body = response.payload.body;

                const pattern = new AppPatternDto(
                    body.patternId,
                    body.name,
                    body.pattern,
                    body.message);

                return new Versioned(response.version, pattern);
            }),
            pretifyError('Failed to add pattern. Please reload.'));
    }

    public putPattern(appName: string, id: string, dto: EditAppPatternDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            pretifyError('Failed to update pattern. Please reload.'));
    }

    public deletePattern(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
            pretifyError('Failed to remove pattern. Please reload.'));
    }
}