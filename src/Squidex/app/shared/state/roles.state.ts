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
    ResourceLinks,
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

    // The links.
    links: ResourceLinks;
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

    public links =
        this.changes.pipe(map(x => x.links),
            distinctUntilChanged());

    constructor(
        private readonly rolesService: RolesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ roles: ImmutableArray.empty(), version: Version.EMPTY, links: {} });
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
        const roles = ImmutableArray.of(payload.items);

        this.next(s => {
            return { ...s, roles, isLoaded: true, version, links: payload._links };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}