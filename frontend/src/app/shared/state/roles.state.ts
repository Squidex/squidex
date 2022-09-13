/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { DialogService, LoadingState, shareSubscribed, State, Version } from '@app/framework';
import { CreateRoleDto, RoleDto, RolesPayload, RolesService, UpdateRoleDto } from './../services/roles.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current roles.
    roles: ReadonlyArray<RoleDto>;

    // The app version.
    version: Version;

    // Indicates if the user can add a role.
    canCreate?: boolean;
}

@Injectable()
export class RolesState extends State<Snapshot> {
    public roles =
        this.project(x => x.roles);

    public defaultRoles =
        this.project(x => x.roles.filter(y => y.isDefaultRole));

    public customRoles =
        this.project(x => x.roles.filter(y => !y.isDefaultRole));

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rolesService: RolesService,
    ) {
        super({ roles: [], version: Version.EMPTY }, 'Roles');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Success');

        return this.rolesService.getRoles(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:roles.reloaded');
                }

                this.replaceRoles(payload, version);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
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
        const { canCreate, items: roles } = payload;

        this.next({
            canCreate,
            isLoaded: true,
            isLoading: false,
            roles,
            version,
        }, 'Loading Success / Updated');
    }

    private get version() {
        return this.snapshot.version;
    }
}
