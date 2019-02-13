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
    pretifyError
} from '@app/framework';

export class FeatureDto {
    constructor(
        public readonly name: string,
        public readonly text: string
    ) {
    }
}

export class FeaturesDto {
    constructor(
        public readonly features: FeatureDto[],
        public readonly version: number
    ) {
    }
}

@Injectable()
export class NewsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getFeatures(): Observable<FeaturesDto> {
        const url = this.apiUrl.buildUrl('api/news/features');

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.features;

                    return new FeaturesDto(
                        items.map(item => {
                            return new FeatureDto(
                                item.name,
                                item.text
                            );
                        }),
                        body.version
                    );
                }),
                pretifyError('Failed to load features. Please reload.'));
    }
}