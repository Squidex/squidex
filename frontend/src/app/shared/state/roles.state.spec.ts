/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, RolesPayload, RolesService, RolesState, versioned } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { createRoles } from './../services/roles.service.spec';
import { TestValues } from './_test-helpers';

describe('RolesState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

    const oldRoles = createRoles(1, 2);

    let dialogs: IMock<DialogService>;
    let rolesService: IMock<RolesService>;
    let rolesState: RolesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rolesService = Mock.ofType<RolesService>();
        rolesState = new RolesState(appsState.object, dialogs.object, rolesService.object);
    });

    describe('Loading', () => {
        it('should load roles', () => {
            rolesService.setup(x => x.getRoles(app))
                .returns(() => of(versioned(version, oldRoles))).verifiable();

            rolesState.load().subscribe();

            expect(rolesState.snapshot.isLoaded).toBeTruthy();
            expect(rolesState.snapshot.isLoading).toBeFalsy();
            expect(rolesState.snapshot.roles).toEqual(oldRoles.items);
            expect(rolesState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            rolesService.setup(x => x.getRoles(app))
                .returns(() => throwError(() => 'Service Error'));

            rolesState.load().pipe(onErrorResumeNext()).subscribe();

            expect(rolesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            rolesService.setup(x => x.getRoles(app))
                .returns(() => of(versioned(version, oldRoles))).verifiable();

            rolesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            rolesService.setup(x => x.getRoles(app))
                .returns(() => of(versioned(version, oldRoles))).verifiable();

            rolesState.load().subscribe();
        });

        it('should update roles if role added', () => {
            const updated = createRoles(4, 5);

            const request = { name: 'newRole' };

            rolesService.setup(x => x.postRole(app, request, version))
                .returns(() => of(versioned(newVersion, updated)));

            rolesState.add(request).subscribe();

            expectNewRoles(updated);
        });

        it('should update roles if role updated', () => {
            const updated = createRoles(4, 5);

            const request = { permissions: ['P4', 'P5'], properties: {} };

            rolesService.setup(x => x.putRole(app, oldRoles.items[1], request, version))
                .returns(() => of(versioned(newVersion, updated)));

            rolesState.update(oldRoles.items[1], request).subscribe();

            expectNewRoles(updated);
        });

        it('should update roles if role deleted', () => {
            const updated = createRoles(4, 5);

            rolesService.setup(x => x.deleteRole(app, oldRoles.items[1], version))
                .returns(() => of(versioned(newVersion, updated)));

            rolesState.delete(oldRoles.items[1]).subscribe();

            expectNewRoles(updated);
        });

        function expectNewRoles(updated: RolesPayload) {
            expect(rolesState.snapshot.roles).toEqual(updated.items);
            expect(rolesState.snapshot.version).toEqual(newVersion);
        }
    });
});
