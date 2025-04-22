/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiUrlConfig, pretifyError, Resource } from '@app/framework';
import { CreateIndexDto, IndexesDto } from './../model';

@Injectable({
    providedIn: 'root',
})
export class IndexesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getIndexes(appName: string, schemaName: string): Observable<IndexesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/indexes`);

        return this.http.get(url).pipe(
            map(body => {
                return IndexesDto.fromJSON(body as any);
            }),
            pretifyError('i18n:schemas.indexes.loadFailed'));
    }

    public postIndex(appName: string, schemaName: string, dto: CreateIndexDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/indexes`);

        return this.http.post(url, dto.toJSON()).pipe(
            pretifyError('i18n:schemas.indexes.createFailed'));
    }

    public deleteIndex(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:schemas.indexes.deleteFailed'));
    }
}

export { CreateIndexDto };
