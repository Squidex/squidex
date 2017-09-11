/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP,
    Version
} from 'framework';

export class AppContributorDto {
    constructor(
        public readonly contributorId: string,
        public readonly permission: string
    ) {
    }

    public changePermission(permission: string): AppContributorDto {
        return new AppContributorDto(this.contributorId, permission);
    }
}

export class AppContributorsDto {
    constructor(
        public readonly contributors: AppContributorDto[],
        public readonly maxContributors: number
    ) {
    }
}

@Injectable()
export class AppContributorsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getContributors(appName: string, version?: Version): Observable<AppContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.getVersioned(this.http, url, version)
                .map(response => {
                    const items: any[] = response.contributors;

                    return new AppContributorsDto(
                        items.map(item => {
                            return new AppContributorDto(
                                item.contributorId,
                                item.permission);
                        }),
                        response.maxContributors);
                })
                .pretifyError('Failed to load contributors. Please reload.');
    }

    public postContributor(appName: string, dto: AppContributorDto, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .pretifyError('Failed to add contributors. Please reload.');
    }

    public deleteContributor(appName: string, contributorId: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors/${contributorId}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to delete contributors. Please reload.');
    }
}