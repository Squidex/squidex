/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    AppClientDto,
    AppClientsService,
    AppComponentBase,
    AppsStoreService,
    CreateAppClientDto,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    NotificationService,
    UpdateAppClientDto,
    ValidatorsEx,
    Version
} from 'shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent extends AppComponentBase implements OnInit {
    private version = new Version();

    public appClients: ImmutableArray<AppClientDto>;

    public addClientFormSubmitted = false;
    public addClientForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]]
        });

    public get hasName() {
        return this.addClientForm.get('name').value && this.addClientForm.get('name').value.length > 0;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly appClientsService: AppClientsService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appClientsService.getClients(app, this.version).retry(2))
            .subscribe(dtos => {
                this.updateClients(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appNameOnce()
            .switchMap(app => this.appClientsService.deleteClient(app, client.id, this.version))
            .subscribe(() => {
                this.updateClients(this.appClients.remove(client));
            }, error => {
                this.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        const request = new UpdateAppClientDto(name);

        this.appNameOnce()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, request, this.version))
            .subscribe(() => {
                this.updateClients(this.appClients.replace(client, rename(client, name)));
            }, error => {
                this.notifyError(error);
            });
    }

    public resetClientForm() {
        this.addClientFormSubmitted = false;
        this.addClientForm.enable();
        this.addClientForm.reset();
    }

    public attachClient() {
        if (this.addClientForm.valid) {
            this.addClientFormSubmitted = true;
            this.addClientForm.disable();

            const requestDto = new CreateAppClientDto(this.addClientForm.get('name')!.value);

            this.appNameOnce()
                .switchMap(app => this.appClientsService.postClient(app, requestDto, this.version))
                .subscribe(dto => {
                    this.updateClients(this.appClients.push(dto));
                    this.resetClientForm();
                }, error => {
                    this.notifyError(error);
                    this.resetClientForm();
                });
        }
    }

    private updateClients(clients: ImmutableArray<AppClientDto>) {
        this.appClients = clients;

        this.messageBus.publish(new HistoryChannelUpdated());
    }
}

function rename(client: AppClientDto, name: string): AppClientDto {
    return new AppClientDto(client.id, name, client.secret);
};
