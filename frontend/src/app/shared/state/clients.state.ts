/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, shareSubscribed, State, VersionTag } from '@app/framework';
import { ClientDto, ClientsDto, ClientsService, CreateClientDto, UpdateClientDto } from '../internal';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current clients.
    clients: ReadonlyArray<ClientDto>;

    // The app version.
    version: VersionTag;

    // Indicates if the user can create new clients.
    canCreate?: boolean;
}

@Service()
export class ClientsState extends State<Snapshot> {
    private readonly appsState = inject(AppsState);
    private readonly clientsService = inject(ClientsService);
    private readonly dialogs = inject(DialogService);

    public readonly clients =
        this.project(x => x.clients);

    public readonly isLoaded =
        this.project(x => x.isLoaded === true);

    public readonly isLoading =
        this.project(x => x.isLoading === true);

    public readonly canCreate =
        this.project(x => x.canCreate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor() {
        super({ clients: [], version: VersionTag.EMPTY });

        debug(this, 'clients');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.clientsService.getClients(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:clients.reloaded');
                }

                this.replaceClients(payload, version);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
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

    public regenerateSecret(client: ClientDto): Observable<any> {
        return this.clientsService.putClientSecret(this.appName, client, this.version).pipe(
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

    private replaceClients(payload: ClientsDto, version: VersionTag) {
        const { canCreate, items: clients } = payload;

        this.next({
            canCreate,
            clients,
            isLoaded: true,
            isLoading: false,
            version,
        }, 'Loading Success / Updated');
    }

    private get version() {
        return this.snapshot.version;
    }
}
