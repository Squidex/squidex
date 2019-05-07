/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    HTTP,
    mapVersioned,
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export type ClientsDto = Versioned<ClientDto[]>;

export class ClientDto extends Model<ClientDto> {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly secret: string,
        public readonly role = 'Developer'
    ) {
        super();
    }
}

export class AccessTokenDto {
    constructor(
        public readonly accessToken: string,
        public readonly tokenType: string
    ) {
    }
}

export interface CreateClientDto {
    readonly id: string;
}

export interface UpdateClientDto {
    readonly name?: string;
    readonly role?: string;
}

@Injectable()
export class ClientsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getClients(appName: string): Observable<ClientsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                mapVersioned(({ body }) => {
                    const items: any[] = body;

                    const clients = items.map(item =>
                        new ClientDto(
                            item.id,
                            item.name || item.id,
                            item.secret,
                            item.role));

                    return clients;
                }),
                pretifyError('Failed to load clients. Please reload.'));
    }

    public postClient(appName: string, dto: CreateClientDto, version: Version): Observable<Versioned<ClientDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
                mapVersioned(({ body }) => {
                    const client = new ClientDto(
                        body.id,
                        body.name || body.id,
                        body.secret,
                        body.role);

                    return client;
                }),
                tap(() => {
                    this.analytics.trackEvent('Client', 'Created', appName);
                }),
                pretifyError('Failed to add client. Please reload.'));
    }

    public putClient(appName: string, id: string, dto: UpdateClientDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Client', 'Updated', appName);
                }),
                pretifyError('Failed to revoke client. Please reload.'));
    }

    public deleteClient(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Client', 'Deleted', appName);
                }),
                pretifyError('Failed to revoke client. Please reload.'));
    }

    public createToken(appName: string, client: ClientDto): Observable<AccessTokenDto> {
        const options = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded', 'NoAuth': 'true'
            })
        };

        const body = `grant_type=client_credentials&scope=squidex-api&client_id=${appName}:${client.id}&client_secret=${client.secret}`;

        const url = this.apiUrl.buildUrl('identity-server/connect/token');

        return this.http.post(url, body, options).pipe(
                map((response: any) => {
                    return new AccessTokenDto(response.access_token, response.token_type);
                }),
                pretifyError('Failed to create token. Please retry.'));
    }
}