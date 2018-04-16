/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppClientDto,
    AppsState,
    AttachClientForm,
    ClientsState,
    CreateAppClientDto
} from '@app/shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent implements OnInit {
    public addClientForm = new AttachClientForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly clientsState: ClientsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.clientsState.load().onErrorResumeNext().subscribe();
    }

    public reload() {
        this.clientsState.load(true).onErrorResumeNext().subscribe();
    }

    public attachClient() {
        const value = this.addClientForm.submit();

        if (value) {
            const requestDto = new CreateAppClientDto(value.name);

            this.clientsState.attach(requestDto).onErrorResumeNext()
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

    public trackByClient(index: number, item: AppClientDto) {
        return item.id;
    }
}