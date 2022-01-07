/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppsState, ClientDto, ClientsState, DialogModel, RoleDto } from '@app/shared';

@Component({
    selector: 'sqx-client[client][clientRoles]',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientComponent implements OnChanges {
    @Input()
    public client!: ClientDto;

    @Input()
    public clientRoles!: ReadonlyArray<RoleDto>;

    public apiCallsLimit = 0;

    public connectDialog = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        private readonly clientsState: ClientsState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['client']) {
            this.apiCallsLimit = this.client.apiCallsLimit;
        }
    }

    public revoke() {
        this.clientsState.revoke(this.client);
    }

    public updateRole(role: string) {
        this.clientsState.update(this.client, { role });
    }

    public updateAllowAnonymous(allowAnonymous: boolean) {
        this.clientsState.update(this.client, { allowAnonymous });
    }

    public updateApiCallsLimit() {
        this.clientsState.update(this.client, { apiCallsLimit: this.client.apiCallsLimit });
    }

    public rename(name: string) {
        this.clientsState.update(this.client, { name });
    }

    public trackByRole(_index: number, role: RoleDto) {
        return role.name;
    }
}
