/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, Version } from 'framework';
import { AuthService } from './auth.service';

export class AppContributorDto {
    constructor(
        public readonly contributorId: string,
        public readonly permission: string
    ) {
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
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getContributors(appName: string, version?: Version): Observable<AppContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
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
                .catchError('Failed to load contributors. Please reload.');
    }

    public postContributor(appName: string, dto: AppContributorDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return this.authService.authPost(url, dto, version)
                .catchError('Failed to add contributors. Please reload.');
    }

    public deleteContributor(appName: string, contributorId: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors/${contributorId}`);

        return this.authService.authDelete(url, version)
                .catchError('Failed to delete contributors. Please reload.');
    }
}