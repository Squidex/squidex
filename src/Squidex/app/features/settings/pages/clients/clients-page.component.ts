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
    UsersProviderService,
    ValidatorsEx
} from 'shared';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent extends AppComponentBase implements OnInit {
    public appClients: ImmutableArray<AppClientDto>;

    public addClientForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appClientsService: AppClientsService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.appClientsService.getClients(app).retry(2))
            .subscribe(dtos => {
                this.updateClients(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appName()
            .switchMap(app => this.appClientsService.deleteClient(app, client.id))
            .subscribe(() => {
                this.updateClients(this.appClients.remove(client));
            }, error => {
                this.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        const request = new UpdateAppClientDto(name);

        this.appName()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, request))
            .subscribe(() => {
                this.updateClients(this.appClients.replace(client, rename(client, name)));
            }, error => {
                this.notifyError(error);
            });
    }

    public resetClientForm() {
        this.addClientForm.reset();
    }

    public attachClient() {
        this.addClientForm.markAsDirty();

        if (this.addClientForm.valid) {
            this.addClientForm.disable();

            const requestDto = new CreateAppClientDto(this.addClientForm.get('name').value);

            const reset = () => {
                this.addClientForm.reset();
                this.addClientForm.enable();
            };

            this.appName()
                .switchMap(app => this.appClientsService.postClient(app, requestDto))
                .subscribe(dto => {
                    this.updateClients(this.appClients.push(dto));
                    reset();
                }, error => {
                    this.notifyError(error);
                    reset();
                });
        }
    }

    private updateClients(clients: ImmutableArray<AppClientDto>) {
        this.appClients = clients;

        this.messageBus.publish(new HistoryChannelUpdated());
    }
}

function rename(client: AppClientDto, name: string): AppClientDto {
    return new AppClientDto(client.id, name, client.secret, client.expiresUtc);
};
