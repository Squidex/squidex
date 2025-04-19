/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { ClientDto, ClientsDto, CreateClientDto, UpdateClientDto } from './../model';

export class AccessTokenDto {
    constructor(
        public readonly accessToken: string,
        public readonly tokenType: string,
    ) {
    }
}

@Injectable({
    providedIn: 'root',
})
export class ClientsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getClients(appName: string): Observable<Versioned<ClientsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return ClientsDto.fromJSON(body);
            }),
            pretifyError('i18n:clients.loadFailed'));
    }

    public postClient(appName: string, dto: CreateClientDto, version: VersionOrTag): Observable<Versioned<ClientsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.postVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return ClientsDto.fromJSON(body);
            }),
            pretifyError('i18n:clients.addFailed'));
    }

    public putClient(appName: string, resource: Resource, dto: UpdateClientDto, version: VersionOrTag): Observable<Versioned<ClientsDto>> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            mapVersioned(({ body }) => {
                return ClientsDto.fromJSON(body);
            }),
            pretifyError('i18n:clients.revokeFailed'));
    }

    public deleteClient(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<ClientsDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return ClientsDto.fromJSON(body);
            }),
            pretifyError('i18n:clients.revokeFailed'));
    }

    public createToken(appName: string, client: ClientDto): Observable<AccessTokenDto> {
        const options = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded', NoAuth: 'true',
            }),
        };

        const body = `grant_type=client_credentials&scope=squidex-api&client_id=${appName}:${client.id}&client_secret=${client.secret}`;

        const url = this.apiUrl.buildUrl('identity-server/connect/token');

        return this.http.post(url, body, options).pipe(
            map((response: any) => {
                return new AccessTokenDto(response.access_token, response.token_type);
            }),
            pretifyError('i18n:clients.tokenFailed'));
    }
}