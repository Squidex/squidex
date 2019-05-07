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

export type PatternsDto = Versioned<PatternDto[]>;

export class PatternDto extends Model<PatternDto> {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly pattern: string,
        public readonly message?: string
    ) {
        super();
    }
}

export interface EditPatternDto {
    readonly name: string;
    readonly pattern: string;
    readonly message?: string;
}

@Injectable()
export class PatternsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getPatterns(appName: string): Observable<PatternsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
            mapVersioned(({ body }) => {
                const items: any[] = body;

                const patterns =
                    items.map(item =>
                        new PatternDto(
                            item.patternId,
                            item.name,
                            item.pattern,
                            item.message));

                return patterns;
            }),
            pretifyError('Failed to add pattern. Please reload.'));
    }

    public postPattern(appName: string, dto: EditPatternDto, version: Version): Observable<Versioned<PatternDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                const pattern = new PatternDto(
                    body.patternId,
                    body.name,
                    body.pattern,
                    body.message);

                return pattern;
            }),
            tap(() => {
                this.analytics.trackEvent('Patterns', 'Created', appName);
            }),
            pretifyError('Failed to add pattern. Please reload.'));
    }

    public putPattern(appName: string, id: string, dto: EditPatternDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Patterns', 'Updated', appName);
            }),
            pretifyError('Failed to update pattern. Please reload.'));
    }

    public deletePattern(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Patterns', 'Configured', appName);
            }),
            pretifyError('Failed to remove pattern. Please reload.'));
    }
}