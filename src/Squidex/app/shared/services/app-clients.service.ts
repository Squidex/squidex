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
    AnalyticsService,
    ApiUrlConfig,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class AppClientsDto {
    constructor(
        public readonly clients: AppClientDto[],
        public readonly version: Version
    ) {
    }

    public addClient(client: AppClientDto, version: Version) {
        return new AppClientsDto([...this.clients, client], version);
    }

    public updateClient(client: AppClientDto, version: Version) {
        return new AppClientsDto(this.clients.map(c => c.id === client.id ? client : c), version);
    }

    public removeClient(client: AppClientDto, version: Version) {
        return new AppClientsDto(this.clients.filter(c => c.id !== client.id), version);
    }
}

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
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getClients(appName: string): Observable<AppClientsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body;

                    const clients = items.map(item => {
                        return new AppClientDto(
                            item.id,
                            item.name || body.id,
                            item.secret,
                            item.isReader);
                    });

                    return new AppClientsDto(clients, response.version);
                })
                .pretifyError('Failed to load clients. Please reload.');
    }

    public postClient(appName: string, dto: CreateAppClientDto, version: Version): Observable<Versioned<AppClientDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.postVersioned<any>(this.http, url, dto, version)
                .map(response => {
                    const body = response.payload.body;

                    const client = new AppClientDto(
                        body.id,
                        body.name || body.id,
                        body.secret,
                        body.isReader);

                    return new Versioned(response.version, client);
                })
                .do(() => {
                    this.analytics.trackEvent('Client', 'Created', appName);
                })
                .pretifyError('Failed to add client. Please reload.');
    }

    public updateClient(appName: string, id: string, dto: UpdateAppClientDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .do(() => {
                    this.analytics.trackEvent('Client', 'Updated', appName);
                })
                .pretifyError('Failed to revoke client. Please reload.');
    }

    public deleteClient(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .do(() => {
                    this.analytics.trackEvent('Client', 'Deleted', appName);
                })
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