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
    AppClientsService,
    AppComponentBase,
    AppsStoreService,
    CreateAppClientDto,
    ImmutableArray,
    NotificationService,
    UpdateAppClientDto,
    UsersProviderService
} from 'shared';

function rename(client: AppClientDto, name: string) {
    return new AppClientDto(client.id, client.secret, name, client.expiresUtc);
};

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html'
})
export class ClientsPageComponent extends AppComponentBase implements OnInit {
    public appClients: ImmutableArray<AppClientDto>;

    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appClientsService: AppClientsService,
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
                this.appClients = ImmutableArray.of(dtos);
            }, error => {
                this.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appName()
            .switchMap(app => this.appClientsService.deleteClient(app, client.id))
            .subscribe(() => {
                this.appClients = this.appClients.remove(client);
            }, error => {
                this.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        this.appName()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, new UpdateAppClientDto(name)))
            .subscribe(() => {
                this.appClients = this.appClients.replace(client, rename(client, name));
            }, error => {
                this.notifyError(error);
            });
    }

    public attachClient() {
        this.createForm.markAsDirty();

        if (this.createForm.valid) {
            this.createForm.disable();

            const dto = new CreateAppClientDto(this.createForm.controls['name'].value);

            this.appName()
                .switchMap(app => this.appClientsService.postClient(app, dto))
                .subscribe(dto => {
                    this.appClients = this.appClients.push(dto);
                    this.reset();
                }, error => {
                    this.notifyError(error);
                    this.reset();
                });
        }
    }

    private reset() {
        this.createForm.reset();
        this.createForm.enable();
    }
}

