/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppClientDto,
    AppClientsDto,
    AppClientsService,
    AppContext,
    CreateAppClientDto,
    HistoryChannelUpdated,
    UpdateAppClientDto,
    ValidatorsEx
} from 'shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html',
    providers: [
        AppContext
    ]
})
export class ClientsPageComponent implements OnInit {
    public appClients: AppClientsDto;

    public addClientFormSubmitted = false;
    public addClientForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]]
        });

    public get hasName() {
        return this.addClientForm.controls['name'].value && this.addClientForm.controls['name'].value.length > 0;
    }

    constructor(public readonly ctx: AppContext,
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appClientsService.getClients(this.ctx.appName)
            .subscribe(dtos => {
                this.updateClients(dtos);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appClientsService.deleteClient(this.ctx.appName, client.id, this.appClients.version)
            .subscribe(dto => {
                this.updateClients(this.appClients.removeClient(client, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        const requestDto = new UpdateAppClientDto(name);

        this.appClientsService.updateClient(this.ctx.appName, client.id, requestDto, this.appClients.version)
            .subscribe(dto => {
                this.updateClients(this.appClients.updateClient(client.rename(name), dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public updateClient(client: AppClientDto, permission: string) {
        const requestDto = new UpdateAppClientDto(undefined, permission);

        this.appClientsService.updateClient(this.ctx.appName, client.id, requestDto, this.appClients.version)
            .subscribe(dto => {
                this.updateClients(this.appClients.updateClient(client.update(permission), dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public attachClient() {
        this.addClientFormSubmitted = true;

        if (this.addClientForm.valid) {
            this.addClientForm.disable();

            const requestDto = new CreateAppClientDto(this.addClientForm.controls['name'].value);

            this.appClientsService.postClient(this.ctx.appName, requestDto, this.appClients.version)
                .subscribe(dto => {
                    this.updateClients(this.appClients.addClient(dto.payload, dto.version));
                    this.resetClientForm();
                }, error => {
                    this.ctx.notifyError(error);
                    this.resetClientForm();
                });
        }
    }

    public cancelAttachClient() {
        this.resetClientForm();
    }

    private resetClientForm() {
        this.addClientFormSubmitted = false;
        this.addClientForm.enable();
        this.addClientForm.reset();
    }

    private updateClients(appClients: AppClientsDto) {
        this.appClients = appClients;

        this.ctx.bus.emit(new HistoryChannelUpdated());
    }
}