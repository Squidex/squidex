/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppClientDto,
    AppClientsDto,
    AppClientsService,
    AppsState,
    ClientsState,
    CreateAppClientDto,
    DialogService,
    UpdateAppClientDto,
    Version,
    Versioned
 } from '@app/shared';

describe('ClientsState', () => {
    const app = 'my-app';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldClients = [
        new AppClientDto('id1', 'name1', 'secret1', 'Developer'),
        new AppClientDto('id2', 'name2', 'secret2', 'Developer')
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let clientsService: IMock<AppClientsService>;
    let clientsState: ClientsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        clientsService = Mock.ofType<AppClientsService>();

        clientsService.setup(x => x.getClients(app))
            .returns(() => of(new AppClientsDto(oldClients, version)));

        clientsState = new ClientsState(clientsService.object, appsState.object, dialogs.object);
        clientsState.load().subscribe();
    });

    it('should load clients', () => {
        expect(clientsState.snapshot.clients.values).toEqual(oldClients);
        expect(clientsState.snapshot.version).toEqual(version);
        expect(clientsState.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        clientsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add client to snapshot when created', () => {
        const newClient = new AppClientDto('id3', 'name3', 'secret3', 'Developer');

        const request = new CreateAppClientDto('id3');

        clientsService.setup(x => x.postClient(app, request, version))
            .returns(() => of(new Versioned<AppClientDto>(newVersion, newClient)));

        clientsState.attach(request).subscribe();

        expect(clientsState.snapshot.clients.values).toEqual([...oldClients, newClient]);
        expect(clientsState.snapshot.version).toEqual(newVersion);
    });

    it('should update properties when updated', () => {
        const request = new UpdateAppClientDto('NewName', 'NewPermission');

        clientsService.setup(x => x.putClient(app, oldClients[0].id, request, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        clientsState.update(oldClients[0], request).subscribe();

        const client_1 = clientsState.snapshot.clients.at(0);

        expect(client_1.name).toBe('NewName');
        expect(client_1.permission).toBe('NewPermission');
        expect(clientsState.snapshot.version).toEqual(newVersion);
    });

    it('should remove client from snapshot when revoked', () => {
        clientsService.setup(x => x.deleteClient(app, oldClients[0].id, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        clientsState.revoke(oldClients[0]).subscribe();

        expect(clientsState.snapshot.clients.values).toEqual([oldClients[1]]);
        expect(clientsState.snapshot.version).toEqual(newVersion);
    });
});