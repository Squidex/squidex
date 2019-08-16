/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    CreateRoleDto,
    RoleDto,
    RolesPayload,
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

    // Indicates if the user can add a role.
    canCreate?: boolean;
}

type RolesList = ImmutableArray<RoleDto>;

@Injectable()
export class RolesState extends State<Snapshot> {
    public roles =
        this.project(x => x.roles);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly rolesService: RolesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ roles: ImmutableArray.empty(), version: Version.EMPTY });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

       return this.rolesService.getRoles(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Roles reloaded.');
                }

                this.replaceRoles(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public add(request: CreateRoleDto): Observable<any> {
        return this.rolesService.postRole(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceRoles(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(role: RoleDto, request: UpdateRoleDto): Observable<any> {
        return this.rolesService.putRole(this.appName, role, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceRoles(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(role: RoleDto): Observable<any> {
        return this.rolesService.deleteRole(this.appName, role, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceRoles(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceRoles(payload: RolesPayload, version: Version) {
        const { canCreate, items } = payload;

        const roles = ImmutableArray.of(items);

        this.next(s => {
            return { ...s, roles, isLoaded: true, version, canCreate };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}