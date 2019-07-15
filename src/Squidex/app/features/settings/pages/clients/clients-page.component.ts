/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddClientForm,
    AppsState,
    ClientDto,
    ClientsState,
    RolesState
} from '@app/shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent implements OnInit {
    public addClientForm = new AddClientForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly clientsState: ClientsState,
        public readonly rolesState: RolesState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.clientsState.load();
    }

    public reload() {
        this.clientsState.load(true);
    }

    public attachClient() {
        const value = this.addClientForm.submit();

        if (value) {
            this.clientsState.attach({ id: value.name })
                .subscribe(() => {
                    this.addClientForm.submitCompleted();
                }, error => {
                    this.addClientForm.submitFailed(error);
                });
        }
    }

    public cancelAttachClient() {
        this.addClientForm.submitCompleted();
    }

    public trackByClient(index: number, item: ClientDto) {
        return item.id;
    }
}