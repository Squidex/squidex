/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, pretifyError, StringHelper } from '@app/framework';
import { SearchResultDto } from './../model';

@Service()
export class SearchService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getResults(appName: string, query: string): Observable<ReadonlyArray<SearchResultDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/search${StringHelper.buildQuery({ query })}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(SearchResultDto.fromJSON);
            }),
            pretifyError('i18n:search.searchFailed'));
    }
}
