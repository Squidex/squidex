/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ClientDto, ClientsState, RolesState } from '@app/shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html',
})
export class ClientsPageComponent implements OnInit {
    constructor(
        public readonly clientsState: ClientsState,
        public readonly rolesState: RolesState,
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.clientsState.load();
    }

    public reload() {
        this.clientsState.load(true);
    }

    public trackByClient(_index: number, client: ClientDto) {
        return client.id;
    }
}
