/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiUrlConfig, pretifyError, StringHelper } from '@app/framework';
import { FeaturesDto } from '../model';

@Service()
export class NewsService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getFeatures(version: number): Observable<FeaturesDto> {
        const url = this.apiUrl.buildUrl(`api/news/features${StringHelper.buildQuery({ version })}`);

        return this.http.get(url).pipe(
            map(body => {
                return FeaturesDto.fromJSON(body);
            }),
            pretifyError('i18n:features.loadFailed'));
    }
}
