/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable } from 'rxjs';

import { ApiUrlConfig } from 'framework';
import { AuthService } from './auth.service';

import { handleError } from './common';

export class AppContributorDto {
    constructor(
        public readonly contributorId: string,
        public readonly permission: string
    ) {
    }
}

@Ng2.Injectable()
export class AppContributorsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getContributors(appName: string): Observable<AppContributorDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {                    
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppContributorDto(
                            item.contributorId, 
                            item.permission);
                    });
                })
                .catch(response => handleError('Failed to load contributors. Please reload.', response));
    }

    public postContributor(appName: string, dto: AppContributorDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return this.authService.authPost(url, dto)
                .catch(response => handleError('Failed to add contributors. Please reload.', response));
    }

    public deleteContributor(appName: string, contributorId: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors/${contributorId}`);
        
        return this.authService.authDelete(url)
                .catch(response => handleError('Failed to delete contributors. Please reload.', response));
    }
}