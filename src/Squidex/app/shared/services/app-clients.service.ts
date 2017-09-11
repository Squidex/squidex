/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP,
    Version
} from 'framework';

export class AppClientDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly secret: string,
        public readonly isReader: boolean
    ) {
    }

    public rename(name: string): AppClientDto {
        return new AppClientDto(this.id, name, this.secret, this.isReader);
    }

    public change(isReader: boolean): AppClientDto {
        return new AppClientDto(this.id, this.name, this.secret, isReader);
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
        public readonly name?: string,
        public readonly isReader?: boolean
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
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getClients(appName: string, version?: Version): Observable<AppClientDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.getVersioned(this.http, url, version)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppClientDto(
                            item.id,
                            item.name || response.id,
                            item.secret,
                            item.isReader);
                    });
                })
                .pretifyError('Failed to load clients. Please reload.');
    }

    public postClient(appName: string, dto: CreateAppClientDto, version: Version): Observable<AppClientDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .map(response => {
                    return new AppClientDto(
                        response.id,
                        response.name || response.id,
                        response.secret,
                        response.isReader);
                })
                .pretifyError('Failed to add client. Please reload.');
    }

    public updateClient(appName: string, id: string, dto: UpdateAppClientDto, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .pretifyError('Failed to revoke client. Please reload.');
    }

    public deleteClient(appName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to revoke client. Please reload.');
    }

    public createToken(appName: string, client: AppClientDto): Observable<AccessTokenDto> {
        const options = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded', 'NoAuth': 'true'
            })
        };

        const body = `grant_type=client_credentials&scope=squidex-api&client_id=${appName}:${client.id}&client_secret=${client.secret}`;

        const url = this.apiUrl.buildUrl('identity-server/connect/token');

        return this.http.post(url, body, options)
                .map((response: any) => {
                    return new AccessTokenDto(response.access_token, response.token_type);
                })
                .pretifyError('Failed to create token. Please retry.');
    }
}