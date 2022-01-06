/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, pretifyError, ResourceLinks } from '@app/framework';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class SearchResultDto {
    public readonly _links: ResourceLinks;

    public readonly url: string;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly type: string,
        public readonly label?: string,
    ) {
        this._links = links;

        this.url = this._links['url'].href;
    }
}

@Injectable()
export class SearchService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getResults(appName: string, query: string): Observable<ReadonlyArray<SearchResultDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/search/?query=${encodeURIComponent(query)}`);

        return this.http.get<ReadonlyArray<any>>(url).pipe(
            map(body => {
                const results = body.map(item =>
                    new SearchResultDto(
                        item._links,
                        item.name,
                        item.type,
                        item.label));

                return results;
            }),
            pretifyError('i18n:search.searchFailed'));
    }
}
