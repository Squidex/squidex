/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import {
    AppsState,
    ClientDto,
    ClientsState,
    DialogModel,
    fadeAnimation,
    RoleDto
} from '@app/shared';

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientComponent {
    @Input()
    public client: ClientDto;

    @Input()
    public clientRoles: ReadonlyArray<RoleDto>;

    public connectDialog = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        private readonly clientsState: ClientsState
    ) {
    }

    public revoke() {
        this.clientsState.revoke(this.client);
    }

    public update(role: string) {
        this.clientsState.update(this.client, { role });
    }

    public rename(name: string) {
        this.clientsState.update(this.client, { name });
    }

    public trackByRole(role: RoleDto) {
        return role.name;
    }
}