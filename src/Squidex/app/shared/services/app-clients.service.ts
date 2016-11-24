/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable } from 'rxjs';

import { ApiUrlConfig, DateTime } from 'framework';
import { AuthService } from './auth.service';

export class AppClientDto {
    constructor(
        public readonly clientName: string,
        public readonly clientSecret: string,
        public readonly expiresUtc: DateTime
    ) {
    }
}

export class AppClientCreateDto {
    constructor(
        public readonly clientName: string
    ) {
    }
}

@Ng2.Injectable()
export class AppClientsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getClients(appName: string): Observable<AppClientDto[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/apps/${appName}/clients`))
                .map(response => response.json())
                .map(response => {                    
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppClientDto(item.clientName, item.clientSecret, DateTime.parseISO_UTC(item.expiresUtc));
                    });
                });
    }

    public postClient(appName: string, client: AppClientCreateDto): Observable<AppClientDto> {
        return this.authService.authPost(this.apiUrl.buildUrl(`api/apps/${appName}/clients`), client)
                .map(response => response.json())
                .map(response => new AppClientDto(response.clientName, response.clientSecret, DateTime.parseISO_UTC(response.expiresUtc)))
                .catch(response => {
                    if (response.status === 400) {
                        return Observable.throw('An client with the same name already exists.');
                    } else {
                        return Observable.throw('A new client could not be created.');
                    }
                });
    }

    public deleteClient(appName: string, name: string): Observable<any> {
        return this.authService.authDelete(this.apiUrl.buildUrl(`api/apps/${appName}/clients/${name}`));
    }
}