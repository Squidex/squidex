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
    AppRoleDto,
    AppRolesService,
    CreateAppRoleDto,
    UpdateAppRoleDto
} from './../services/app-roles.service';

interface Snapshot {
    roles: ImmutableArray<AppRoleDto>;

    version: Version;

    isLoaded?: boolean;
}

@Injectable()
export class RolesState extends State<Snapshot> {
    public roles =
        this.changes.pipe(map(x => x.roles),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appRolesService: AppRolesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ roles: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.appRolesService.getRoles(this.appName).pipe(
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Roles reloaded.');
                }

                this.next(s => {
                    const roles = ImmutableArray.of(dtos.roles).sortByStringAsc(x => x.name);

                    return { ...s, roles, isLoaded: true, version: dtos.version };
                });
            }),
            notify(this.dialogs));
    }

    public add(request: CreateAppRoleDto): Observable<any> {
        return this.appRolesService.postRole(this.appName, request, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const roles = s.roles.push(dto.payload).sortByStringAsc(x => x.name);

                    return { ...s, roles, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    public delete(role: AppRoleDto): Observable<any> {
        return this.appRolesService.deleteRole(this.appName, role.name, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const roles = s.roles.filter(c => c.name !== role.name);

                    return { ...s, roles, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    public update(role: AppRoleDto, request: UpdateAppRoleDto): Observable<any> {
        return this.appRolesService.putRole(this.appName, role.name, request, this.version).pipe(
            tap(dto => {
                this.next(s => {
                    const roles = s.roles.replaceBy('name', update(role, request));

                    return { ...s, roles, version: dto.version };
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

const update = (role: AppRoleDto, request: UpdateAppRoleDto) =>
    role.with({ permissions: request.permissions });