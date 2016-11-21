/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Http from '@angular/http';

import { Observable } from 'rxjs';

import { ApiUrlConfig, DateTime } from 'framework';
import { AuthService } from './auth.service';

export class AppClientKeyDto {
    constructor(
        public readonly clientKey: string,
        public readonly expiresUtc: DateTime
    ) {
    }
}

@Ng2.Injectable()
export class AppClientKeysService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly http: Ng2Http.Http
    ) {
    }

    public getClientKeys(appName: string): Observable<AppClientKeyDto[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/apps/${appName}/client-keys`))
                .map(response => {                    
                    const body: any[] = response.json();

                    return body.map(item => {
                        return new AppClientKeyDto(item.clientKey, DateTime.parseISO_UTC(item.expiresUtc));
                    });
                });
    }

    public postClientKey(appName: string): Observable<AppClientKeyDto> {
        return this.authService.authPost(this.apiUrl.buildUrl(`api/apps/${appName}/client-keys`), {})
                .map(response => {
                    const body = response.json();

                    return new AppClientKeyDto(body.clientKey, DateTime.now().addYears(1));
                });
    }
}