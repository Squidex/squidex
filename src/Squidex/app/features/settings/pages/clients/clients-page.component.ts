/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppClientDto,
    AppClientsDto,
    AppClientsService,
    AppComponentBase,
    AppsStoreService,
    AuthService,
    CreateAppClientDto,
    DialogService,
    HistoryChannelUpdated,
    MessageBus,
    UpdateAppClientDto,
    ValidatorsEx
} from 'shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent extends AppComponentBase implements OnInit {
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

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly appClientsService: AppClientsService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appClientsService.getClients(app).retry(2))
            .subscribe(dtos => {
                this.updateClients(dtos);
            }, error => {
                this.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appNameOnce()
            .switchMap(app => this.appClientsService.deleteClient(app, client.id, this.appClients.version))
            .subscribe(dto => {
                this.updateClients(this.appClients.removeClient(client, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        const requestDto = new UpdateAppClientDto(name);

        this.appNameOnce()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, requestDto, this.appClients.version))
            .subscribe(dto => {
                this.updateClients(this.appClients.updateClient(client.rename(name), dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public changeClient(client: AppClientDto, isReader: boolean) {
        const requestDto = new UpdateAppClientDto(undefined, isReader);

        this.appNameOnce()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, requestDto, this.appClients.version))
            .subscribe(dto => {
                this.updateClients(this.appClients.updateClient(client.change(isReader), dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public attachClient() {
        this.addClientFormSubmitted = true;

        if (this.addClientForm.valid) {
            this.addClientForm.disable();

            const requestDto = new CreateAppClientDto(this.addClientForm.controls['name'].value);

            this.appNameOnce()
                .switchMap(app => this.appClientsService.postClient(app, requestDto, this.appClients.version))
                .subscribe(dto => {
                    this.updateClients(this.appClients.addClient(dto.payload, dto.version));
                }, error => {
                    this.notifyError(error);
                }, () => {
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

        this.messageBus.emit(new HistoryChannelUpdated());
    }
}