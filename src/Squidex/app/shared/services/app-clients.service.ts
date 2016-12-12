/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Http from '@angular/http';
import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    DateTime,
    handleError
} from 'framework';

import { AuthService } from './auth.service';

export class AppClientDto {
    constructor(
        public readonly id: string,
        public readonly secret: string,
        public readonly name: string,
        public readonly expiresUtc: DateTime
    ) {
    }
}

export class CreateAppClientDto {
    constructor(
        public readonly id: string
    ) {
    }
}

export class UpdateAppClientDto {
    constructor(
        public readonly name: string
    ) {
    }
}

export class AccessTokenDto {
    constructor(
        public readonly accessToken: string,
        public readonly tokenType: string
    ) {
    }
}

@Ng2.Injectable()
export class AppClientsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly http: Ng2Http.Http
    ) {
    }

    public getClients(appName: string): Observable<AppClientDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppClientDto(
                            item.id,
                            item.secret,
                            item.name,
                            DateTime.parseISO_UTC(item.expiresUtc));
                    });
                })
                .catch(response => handleError('Failed to load clients. Please reload.', response));
    }

    public postClient(appName: string, dto: CreateAppClientDto): Observable<AppClientDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new AppClientDto(
                        response.id,
                        response.secret,
                        response.name,
                        DateTime.parseISO_UTC(response.expiresUtc));
                })
                .catch(response => handleError('Failed to add client. Please reload.', response));
    }

    public updateClient(appName: string, id: string, dto: UpdateAppClientDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return this.authService.authPut(url, dto)
                .catch(response => handleError('Failed to revoke client. Please reload.', response));
    }

    public deleteClient(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return this.authService.authDelete(url)
                .catch(response => handleError('Failed to revoke client. Please reload.', response));
    }

    public createToken(appName: string, client: AppClientDto): Observable<AccessTokenDto> {
        const options = new Ng2Http.RequestOptions({
            headers: new Ng2Http.Headers({
                'Content-Type': 'application/x-www-form-urlencoded'
            })
        });

        const body = `grant_type=client_credentials&scope=squidex-api&client_id=${appName}:${client.id}&client_secret=${client.secret}`;

        const url = this.apiUrl.buildUrl('identity-server/connect/token');

        return this.http.post(url, body, options)
                .map(response => response.json())
                .map(response => new AccessTokenDto(response.access_token, response.token_type));
    }
}