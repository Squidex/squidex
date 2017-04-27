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

@Injectable()
export class AppContributorsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getContributors(appName: string, version?: Version): Observable<AppContributorDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppContributorDto(
                            item.contributorId,
                            item.permission);
                    });
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