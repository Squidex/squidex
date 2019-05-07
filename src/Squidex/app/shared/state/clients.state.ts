/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: no-shadowed-variable

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    mapVersioned,
    shareMapSubscribed,
    shareSubscribed,
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

        return this.clientsService.getClients(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Clients reloaded.');
                }

                const clients = ImmutableArray.of(payload);

                this.next(s => {
                    return { ...s, clients, isLoaded: true, version };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public attach(request: CreateClientDto): Observable<ClientDto> {
        return this.clientsService.postClient(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.next(s => {
                    const clients = s.clients.push(payload);

                    return { ...s, clients, version: version };
                });
            }),
            shareMapSubscribed(this.dialogs, x => x.payload));
    }

    public revoke(client: ClientDto): Observable<any> {
        return this.clientsService.deleteClient(this.appName, client.id, this.version).pipe(
            tap(({ version }) => {
                this.next(s => {
                    const clients = s.clients.filter(c => c.id !== client.id);

                    return { ...s, clients, version };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public update(client: ClientDto, request: UpdateClientDto): Observable<ClientDto> {
        return this.clientsService.putClient(this.appName, client.id, request, this.version).pipe(
            mapVersioned(() => update(client, request)),
            tap(({ version, payload }) => {
                this.next(s => {
                    const clients = s.clients.replaceBy('id', payload);

                    return { ...s, clients, version };
                });
            }),
            shareMapSubscribed(this.dialogs, x => x.payload));
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