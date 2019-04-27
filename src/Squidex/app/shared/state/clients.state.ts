/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: no-shadowed-variable

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';

import {
    array,
    DialogService,
    ImmutableArray,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    ClientDto,
    ClientsService,
    CreateClientDto,
    UpdateClientDto
} from './../services/clients.service';

interface Snapshot {
    // The current clients.
    clients: ClientsList;

    // The app version.
    version: Version;

    // Indicates if the clients are loaded.
    isLoaded?: boolean;
}

type ClientsList = ImmutableArray<ClientDto>;

@Injectable()
export class ClientsState extends State<Snapshot> {
    public clients =
        this.changes.pipe(map(x => x.clients),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly clientsService: ClientsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ clients: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        const stream =
            this.clientsService.getClients(this.appName).pipe(
                map(({ version, clients }) => ({ version, clients: array(clients) })), share());

        stream.subscribe(({ version, clients }) => {
            if (isReload) {
                this.dialogs.notifyInfo('Clients reloaded.');
            }

            this.next(s => {
                return { ...s, clients, isLoaded: true, version };
            });
        });

        return stream;
    }

    public attach(request: CreateClientDto): Observable<ClientDto> {
        const stream =
            this.clientsService.postClient(this.appName, request, this.version).pipe(
                share());

        stream.subscribe(dto => {
            this.next(s => {
                const clients = s.clients.push(dto.payload);

                return { ...s, clients, version: dto.version };
            });
        });

        return stream.pipe(map(x => x.payload));
    }

    public revoke(client: ClientDto): Observable<any> {
        const stream =
            this.clientsService.deleteClient(this.appName, client.id, this.version).pipe(
                share());

        stream.subscribe(({ version }) => {
            this.next(s => {
                const clients = s.clients.filter(c => c.id !== client.id);

                return { ...s, clients, version };
            });
        });

        return stream;
    }

    public update(client: ClientDto, request: UpdateClientDto): Observable<ClientDto> {
        const stream =
            this.clientsService.putClient(this.appName, client.id, request, this.version).pipe(
                map(({ version }) => ({ version, client: update(client, request) })), share());

        stream.subscribe(({ version, client }) => {
            this.next(s => {
                const clients = s.clients.replaceBy('id', client);

                return { ...s, clients, version };
            });
        });

        return stream.pipe(map(x => x.client));
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (client: ClientDto, request: UpdateClientDto) =>
    client.with({ name: request.name || client.name, role: request.role || client.role });