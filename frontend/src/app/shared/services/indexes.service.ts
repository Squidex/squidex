/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiUrlConfig, hasAnyLink, pretifyError, Resource, ResourceLinks } from '@app/framework';

export type IndexField = { name: string; order: 'Ascending' | 'Descending' };

export class IndexDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly fields: ReadonlyArray<IndexField>,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
    }
}

export type IndexesDto = Readonly<{
    // The indexes.
    items: ReadonlyArray<IndexDto>;

    // The if the user can create a new index.
    canCreate?: boolean;
}>;

export type CreateIndexDto = Readonly<{
    // The index fields.
    fields: IndexField[];
}>;

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
                return parseIndexes(body as any);
            }),
            pretifyError('i18n:schemas.indexes.loadFailed'));
    }

    public postIndex(appName: string, schemaName: string, dto: CreateIndexDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/indexes`);

        return this.http.post(url, dto).pipe(
            pretifyError('i18n:schemas.indexes.createFailed'));
    }

    public deleteIndex(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:schemas.indexes.deleteFailed'));
    }
}

function parseIndexes(response: { items: any[] } & Resource): IndexesDto {
    const { items: list, _links } = response;
    const items = list.map(parseIndex);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
}

function parseIndex(response: any) {
    return new IndexDto(response._links,
        response.name,
        response.fields);
}
