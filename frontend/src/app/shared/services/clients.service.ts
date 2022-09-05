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
import { ApiUrlConfig, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';

export class ClientDto {
    public readonly _links: ResourceLinks;

    public readonly canRevoke: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly name: string,
        public readonly secret: string,
        public readonly role: string,
        public readonly apiCallsLimit: number,
        public readonly apiTrafficLimit: number,
        public readonly allowAnonymous: boolean,
    ) {
        this._links = links;

        this.canRevoke = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class AccessTokenDto {
    constructor(
        public readonly accessToken: string,
        public readonly tokenType: string,
    ) {
    }
}

export type ClientsDto = Versioned<ClientsPayload>;

export type ClientsPayload = Readonly<{
    // The list of clients.
    items: ReadonlyArray<ClientDto>;

    // True if the user has permissions to create a client.
    canCreate?: boolean;
}>;

export type CreateClientDto = Readonly<{
    // The new client ID.
    id: string;
 }>;

export type UpdateClientDto = Readonly<{
    // The optional client name.
    name?: string;

    // The role for the client to define the permissions.
    role?: string;

    // True if the client can be used for anonymous access.
    allowAnonymous?: boolean;

    // The allowed api calls.
    apiCallsLimit?: number;
}>;

@Injectable()
export class ClientsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getClients(appName: string): Observable<ClientsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseClients(body);
            }),
            pretifyError('i18n:clients.loadFailed'));
    }

    public postClient(appName: string, dto: CreateClientDto, version: Version): Observable<ClientsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/clients`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseClients(body);
            }),
            pretifyError('i18n:clients.addFailed'));
    }

    public putClient(appName: string, resource: Resource, dto: UpdateClientDto, version: Version): Observable<ClientsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseClients(body);
            }),
            pretifyError('i18n:clients.revokeFailed'));
    }

    public deleteClient(appName: string, resource: Resource, version: Version): Observable<ClientsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseClients(body);
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

function parseClients(response: { items: any[] } & Resource): ClientsPayload {
    const { items: list, _links } = response;
    const items = list.map(parseClient);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
}

function parseClient(response: any) {
    return new ClientDto(response._links,
        response.id,
        response.name || response.id,
        response.secret,
        response.role,
        response.apiCallsLimit,
        response.apiTrafficLimit,
        response.allowAnonymous);
}
