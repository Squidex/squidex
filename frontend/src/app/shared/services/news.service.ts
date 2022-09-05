/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, pretifyError } from '@app/framework';

export type FeatureDto = Readonly<{
    // The name of the feature.
    name: string;

    // The feature description.
    text: string;
}>;

export type FeaturesDto = Readonly<{
    // The list of features.
    features: ReadonlyArray<FeatureDto>;

    // The latest version.
    version: number;
}>;

@Injectable()
export class NewsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getFeatures(version: number): Observable<FeaturesDto> {
        const url = this.apiUrl.buildUrl(`api/news/features?version=${version}`);

        return this.http.get<FeaturesDto>(url).pipe(
            pretifyError('i18n:features.loadFailed'));
    }
}
