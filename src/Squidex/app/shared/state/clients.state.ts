/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    notify,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    AppClientDto,
    AppClientsService,
    CreateAppClientDto,
    UpdateAppClientDto
} from './../services/app-clients.service';

interface Snapshot {
    clients: ImmutableArray<AppClientDto>;

    version: Version;

    isLoaded?: boolean;
}

@Injectable()
export class ClientsState extends State<Snapshot> {
    public clients =
        this.changes.pipe(map(x => x.clients),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appClientsService: AppClientsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ clients: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.appClientsService.getClients(this.appName).pipe(
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Clients reloaded.');
                }

                this.next(s => {
                    const clients = ImmutableArray.of(dtos.clients);

                    return { ...s, clients, isLoaded: true, version: dtos.version };
                });
            }),
            notify(this.dialogs));
    }

    public attach(request: CreateAppClientDto): Observable<any> {
        return this.appClientsService.postClient(this.appName, request, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const clients = s.clients.push(dto.payload);

                    return { ...s, clients, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    public revoke(client: AppClientDto): Observable<any> {
        return this.appClientsService.deleteClient(this.appName, client.id, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const clients = s.clients.filter(c => c.id !== client.id);

                    return { ...s, clients, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    public update(client: AppClientDto, request: UpdateAppClientDto): Observable<any> {
        return this.appClientsService.putClient(this.appName, client.id, request, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const clients = s.clients.replaceBy('id', update(client, request));

                    return { ...s, clients, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (client: AppClientDto, request: UpdateAppClientDto) =>
    client.with({ name: request.name || client.name, permission: request.permission || client.permission });