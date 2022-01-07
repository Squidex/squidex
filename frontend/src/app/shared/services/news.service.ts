/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, pretifyError } from '@app/framework';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class FeatureDto {
    constructor(
        public readonly name: string,
        public readonly text: string,
    ) {
    }
}

export class FeaturesDto {
    constructor(
        public readonly features: ReadonlyArray<FeatureDto>,
        public readonly version: number,
    ) {
    }
}

@Injectable()
export class NewsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getFeatures(version: number): Observable<FeaturesDto> {
        const url = this.apiUrl.buildUrl(`api/news/features?version=${version}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const items: any[] = body.features;

                const features = new FeaturesDto(
                    items.map(item =>
                        new FeatureDto(
                            item.name,
                            item.text),
                    ),
                    body.version);

                return features;
            }),
            pretifyError('i18n:features.loadFailed'));
    }
}
