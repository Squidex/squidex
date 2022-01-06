/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig } from '@app/framework';
import { Observable } from 'rxjs';

@Injectable()
export class GraphQlService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public query(appName: string, params: any): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/content/${appName}/graphql`);

        return this.http.post(url, params);
    }
}
