/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ClientsPayload,
    ClientsService,
    ClientsState,
    DialogService,
    versioned
} from '@app/shared/internal';

import { createClients } from '../services/clients.service.spec';

import { TestValues } from './_test-helpers';

describe('ClientsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldClients = createClients(1, 2);

    let dialogs: IMock<DialogService>;
    let clientsService: IMock<ClientsService>;
    let clientsState: ClientsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        clientsService = Mock.ofType<ClientsService>();
        clientsState = new ClientsState(clientsService.object, appsState.object, dialogs.object);
    });

    afterEach(() => {
        clientsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load clients', () => {
            clientsService.setup(x => x.getClients(app))
                .returns(() => of(versioned(version, oldClients))).verifiable();

            clientsState.load().subscribe();

            expect(clientsState.snapshot.clients.values).toEqual(oldClients.items);
            expect(clientsState.snapshot.version).toEqual(version);
            expect(clientsState.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            clientsService.setup(x => x.getClients(app))
                .returns(() => of(versioned(version, oldClients))).verifiable();

            clientsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            clientsService.setup(x => x.getClients(app))
                .returns(() => of(versioned(version, oldClients))).verifiable();

            clientsState.load().subscribe();
        });

        it('should update clients when client added', () => {
            const updated = createClients(1, 2, 3);

            const request = { id: 'id3' };

            clientsService.setup(x => x.postClient(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            clientsState.attach(request).subscribe();

            expectNewClients(updated);
        });

        it('should update clients when role updated', () => {
            const updated = createClients(1, 2, 3);

            const request = { role: 'Owner' };

            clientsService.setup(x => x.putClient(app, oldClients.items[0], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            clientsState.update(oldClients.items[0], request).subscribe();

            expectNewClients(updated);
        });

        it('should update clients when name updated', () => {
            const updated = createClients(1, 2, 3);

            const request = { name: 'NewName' };

            clientsService.setup(x => x.putClient(app, oldClients.items[0], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            clientsState.update(oldClients.items[0], request).subscribe();

            expectNewClients(updated);
        });

        it('should update clients when client revoked', () => {
            const updated = createClients(1, 2, 3);

            clientsService.setup(x => x.deleteClient(app, oldClients.items[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            clientsState.revoke(oldClients.items[0]).subscribe();

            expectNewClients(updated);
        });

        function expectNewClients(updated: ClientsPayload) {
            expect(clientsState.snapshot.clients.values).toEqual(updated.items);
            expect(clientsState.snapshot.version).toEqual(newVersion);

        }
    });
});