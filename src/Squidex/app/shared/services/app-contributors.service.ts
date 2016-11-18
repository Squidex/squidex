
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

export class AppContributor {
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

    public getContributors(appName: string): Observable<AppContributor[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/apps/${appName}/contributors`))
                .map(response => {                    
                    const body: any[] = response.json();

                    return body.map(item => {
                        return new AppContributor(
                            item.contributorId, 
                            item.permission);
                    });
                });
    }

    public postContributor(appName: string, contributor: AppContributor): Observable<any> {
        return this.authService.authPost(this.apiUrl.buildUrl(`api/apps/${appName}/contributors`), contributor);
    }

    public deleteContributor(appName: string, contributorId: string): Observable<any> {
        return this.authService.authDelete(this.apiUrl.buildUrl(`api/apps/${appName}/contributors/{contributorId}`));
    }
}