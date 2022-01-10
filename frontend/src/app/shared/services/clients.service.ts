/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

export class ClientDto {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;
    public readonly canRevoke: boolean;

    constructor(
        links: ResourceLinks,
        public readonly id: string,
        public readonly name: string,
        public readonly secret: string,
        public readonly role: string,
        public readonly apiCallsLimit: number,
        public readonly apiTrafficLimit: number,
        public readonly allowAnonymous: boolean,
    ) {
        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
        this.canRevoke = hasAnyLink(links, 'delete');
    }
}

export class AccessTokenDto {
    constructor(
        public readonly accessToken: string,
        public readonly tokenType: string,
    ) {
    }
}

export type ClientsDto =
    Versioned<ClientsPayload>;

export type ClientsPayload =
    Readonly<{ items: ReadonlyArray<ClientDto>; canCreate: boolean } & Resource>;

export type CreateClientDto =
    Readonly<{ id: string }>;

export type UpdateClientDto =
    Readonly<{ name?: string; role?: string; allowAnonymous?: boolean; apiCallsLimit?: number }>;

@Injectable()
export class ClientsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
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
            tap(() => {
                this.analytics.trackEvent('Client', 'Created', appName);
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
            tap(() => {
                this.analytics.trackEvent('Client', 'Updated', appName);
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
            tap(() => {
                this.analytics.trackEvent('Client', 'Deleted', appName);
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

function parseClients(response: any): ClientsPayload {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new ClientDto(item._links,
            item.id,
            item.name || item.id,
            item.secret,
            item.role,
            item.apiCallsLimit,
            item.apiTrafficLimit,
            item.allowAnonymous));

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}
