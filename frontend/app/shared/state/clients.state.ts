/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: no-shadowed-variable

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    ClientDto,
    ClientsPayload,
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

    // Indicates if the user can create new clients.
    canCreate?: boolean;
}

type ClientsList = ReadonlyArray<ClientDto>;

@Injectable()
export class ClientsState extends State<Snapshot> {
    public clients =
        this.project(x => x.clients);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    constructor(
        private readonly clientsService: ClientsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ clients: [], version: Version.EMPTY });
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

                this.replaceClients(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public attach(request: CreateClientDto): Observable<any> {
        return this.clientsService.postClient(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceClients(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public revoke(client: ClientDto): Observable<any> {
        return this.clientsService.deleteClient(this.appName, client, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceClients(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(client: ClientDto, request: UpdateClientDto): Observable<any> {
        return this.clientsService.putClient(this.appName, client, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceClients(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceClients(payload: ClientsPayload, version: Version) {
        const { canCreate, items: clients } = payload;

        this.next(s => {
            return {
                ...s,
                canCreate,
                clients,
                isLoaded: true,
                version
            };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}