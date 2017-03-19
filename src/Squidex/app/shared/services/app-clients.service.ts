/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Headers, Http, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, Version } from 'framework';
import { AuthService } from './auth.service';

export class AppClientDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly secret: string
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

@Injectable()
export class AppClientsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly http: Http
    ) {
    }

    public getClients(appName: string, version: Version): Observable<AppClientDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppClientDto(
                            item.id,
                            item.name,
                            item.secret);
                    });
                })
                .catchError('Failed to load clients. Please reload.');
    }

    public postClient(appName: string, dto: CreateAppClientDto, version: Version): Observable<AppClientDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return this.authService.authPost(url, dto, version)
                .map(response => response.json())
                .map(response => {
                    return new AppClientDto(
                        response.id,
                        response.name,
                        response.secret);
                })
                .catchError('Failed to add client. Please reload.');
    }

    public updateClient(appName: string, id: string, dto: UpdateAppClientDto, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return this.authService.authPut(url, dto, version)
                .catchError('Failed to revoke client. Please reload.');
    }

    public deleteClient(appName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return this.authService.authDelete(url, version)
                .catchError('Failed to revoke client. Please reload.');
    }

    public createToken(appName: string, client: AppClientDto): Observable<AccessTokenDto> {
        const options = new RequestOptions({
            headers: new Headers({
                'Content-Type': 'application/x-www-form-urlencoded'
            })
        });

        const body = `grant_type=client_credentials&scope=squidex-api&client_id=${appName}:${client.id}&client_secret=${client.secret}`;

        const url = this.apiUrl.buildUrl('identity-server/connect/token');

        return this.http.post(url, body, options)
                .map(response => response.json())
                .map(response => new AccessTokenDto(response.access_token, response.token_type))
                .catchError('Failed to create token. Please retry.');
    }
}