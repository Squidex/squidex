/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    DialogService,
    RoleDto,
    RolesService,
    RolesState,
    versioned
} from '@app/shared/internal';

import { TestValues } from './_test-helpers';

describe('RolesState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldRoles = [
        new RoleDto('Role1', 3, 5, ['P1']),
        new RoleDto('Role2', 7, 9, ['P2'])
    ];

    let dialogs: IMock<DialogService>;
    let rolesService: IMock<RolesService>;
    let rolesState: RolesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rolesService = Mock.ofType<RolesService>();
        rolesState = new RolesState(rolesService.object, appsState.object, dialogs.object);
    });

    describe('Loading', () => {
        it('should load roles', () => {
            rolesService.setup(x => x.getRoles(app))
                .returns(() => of({ payload: oldRoles, version })).verifiable();

            rolesState.load().subscribe();

            expect(rolesState.snapshot.roles.values).toEqual(oldRoles);
            expect(rolesState.snapshot.isLoaded).toBeTruthy();
            expect(rolesState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
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

        it('should add role to snapshot when added', () => {
            const newRole = new RoleDto('Role3', 0, 0, ['P3']);

            const request = { name: newRole.name };

            rolesService.setup(x => x.postRole(app, request, version))
                .returns(() => of(versioned(newVersion, newRole)));

            rolesState.add(request).subscribe();

            expect(rolesState.snapshot.roles.values).toEqual([oldRoles[0], oldRoles[1], newRole]);
            expect(rolesState.snapshot.version).toEqual(newVersion);
        });

        it('should update permissions when updated', () => {
            const request = { permissions: ['P4', 'P5'] };

            rolesService.setup(x => x.putRole(app, oldRoles[1].name, request, version))
                .returns(() => of(versioned(newVersion)));

            rolesState.update(oldRoles[1], request).subscribe();

            const role_1 = rolesState.snapshot.roles.at(1);

            expect(role_1.permissions).toEqual(request.permissions);
            expect(rolesState.snapshot.version).toEqual(newVersion);
        });

        it('should remove role from snapshot when deleted', () => {
            rolesService.setup(x => x.deleteRole(app, oldRoles[0].name, version))
                .returns(() => of(versioned(newVersion)));

            rolesState.delete(oldRoles[0]).subscribe();

            expect(rolesState.snapshot.roles.values).toEqual([oldRoles[1]]);
            expect(rolesState.snapshot.version).toEqual(newVersion);
        });
    });
});