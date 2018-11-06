/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppRoleDto,
    AppRolesDto,
    AppRolesService,
    AppsState,
    DialogService,
    RolesState,
    Version,
    Versioned
} from '@app/shared';
import { CreateAppRoleDto, UpdateAppRoleDto } from '../services/app-roles.service';

describe('RolesState', () => {
    const app = 'my-app';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldRoles = [
        new AppRoleDto('Role1', 3, 5, ['P1']),
        new AppRoleDto('Role2', 7, 9, ['P2'])
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let rolesService: IMock<AppRolesService>;
    let rolesState: RolesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        rolesService = Mock.ofType<AppRolesService>();

        rolesService.setup(x => x.getRoles(app))
            .returns(() => of(new AppRolesDto(oldRoles, version)));

        rolesState = new RolesState(rolesService.object, appsState.object, dialogs.object);
        rolesState.load().subscribe();
    });

    it('should load roles', () => {
        expect(rolesState.snapshot.roles.values).toEqual(oldRoles);
        expect(rolesState.snapshot.isLoaded).toBeTruthy();
        expect(rolesState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        rolesState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add role to snapshot when added', () => {
        const newRole = new AppRoleDto('Role3', 0, 0, ['P3']);

        const request = new CreateAppRoleDto('Role3');

        rolesService.setup(x => x.postRole(app, request, version))
            .returns(() => of(new Versioned<AppRoleDto>(newVersion, newRole)));

        rolesState.add(request).subscribe();

        expect(rolesState.snapshot.roles.values).toEqual([oldRoles[0], oldRoles[1], newRole]);
        expect(rolesState.snapshot.version).toEqual(newVersion);
    });

    it('should update permissions when updated', () => {
        const request = new UpdateAppRoleDto(['P4', 'P5']);

        rolesService.setup(x => x.putRole(app, oldRoles[1].name, request, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rolesState.update(oldRoles[1], request).subscribe();

        const role_1 = rolesState.snapshot.roles.at(1);

        expect(role_1.permissions).toEqual(request.permissions);
        expect(rolesState.snapshot.version).toEqual(newVersion);
    });

    it('should remove role from snapshot when deleted', () => {
        rolesService.setup(x => x.deleteRole(app, oldRoles[0].name, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rolesState.delete(oldRoles[0]).subscribe();

        expect(rolesState.snapshot.roles.values).toEqual([oldRoles[1]]);
        expect(rolesState.snapshot.version).toEqual(newVersion);
    });
});