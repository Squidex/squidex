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
    mapVersioned,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    CreateRoleDto,
    RoleDto,
    RolesService,
    UpdateRoleDto
} from './../services/roles.service';

interface Snapshot {
    // The current roles.
    roles: RolesList;

    // The app version.
    version: Version;

    // Indicates if the roles are loaded.
    isLoaded?: boolean;
}

type RolesList = ImmutableArray<RoleDto>;

@Injectable()
export class RolesState extends State<Snapshot> {
    public roles =
        this.changes.pipe(map(x => x.roles),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly rolesService: RolesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ roles: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

       return this.rolesService.getRoles(this.appName).pipe(
            tap(({ payload, version }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Roles reloaded.');
                }

                this.next(s => {
                    const roles = ImmutableArray.of(payload).sortByStringAsc(x => x.name);

                    return { ...s, roles, isLoaded: true, version };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public add(request: CreateRoleDto): Observable<RoleDto> {
        return this.rolesService.postRole(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.next(s => {
                    const roles = s.roles.push(payload).sortByStringAsc(x => x.name);

                    return { ...s, roles, version };
                });
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public delete(role: RoleDto): Observable<any> {
        return this.rolesService.deleteRole(this.appName, role.name, this.version).pipe(
            tap(({ version }) => {
                this.next(s => {
                    const roles = s.roles.removeBy('name', role);

                    return { ...s, roles, version };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public update(role: RoleDto, request: UpdateRoleDto): Observable<RoleDto> {
        return this.rolesService.putRole(this.appName, role.name, request, this.version).pipe(
            mapVersioned(() => update(role, request)),
            tap(({ version, payload }) => {
                this.next(s => {
                    const roles = s.roles.replaceBy('name', payload);

                    return { ...s, roles, version };
                });
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (role: RoleDto, request: UpdateRoleDto) =>
    role.with({ permissions: request.permissions });