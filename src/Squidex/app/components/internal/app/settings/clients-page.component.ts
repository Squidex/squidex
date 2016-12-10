/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import {
    AppClientDto,
    AppClientsService,
    AppComponentBase,
    AppsStoreService,
    ArrayHelper,
    CreateAppClientDto,
    NotificationService,
    UpdateAppClientDto,
    UsersProviderService
} from 'shared';

function rename(client: AppClientDto, name: string) {
    return new AppClientDto(client.id, client.secret, name, client.expiresUtc);
};

@Ng2.Component({
    selector: 'sqx-clients-page',
    styles,
    template
})
export class ClientsPageComponent extends AppComponentBase implements Ng2.OnInit {
    public appClients: AppClientDto[];
    
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Ng2Forms.Validators.required,
                    Ng2Forms.Validators.maxLength(40),
                    Ng2Forms.Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: Ng2Forms.FormBuilder
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
                this.appClients = dtos;
            }, error => {
                this.notifyError(error);
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appName()
            .switchMap(app => this.appClientsService.deleteClient(app, client.id))
            .subscribe(() => {
                this.appClients = ArrayHelper.remove(this.appClients, client);
            }, error => {
                this.notifyError(error);
            });
    }

    public renameClient(client: AppClientDto, name: string) {
        this.appName()
            .switchMap(app => this.appClientsService.updateClient(app, client.id, new UpdateAppClientDto(name)))
            .subscribe(() => {
                this.appClients = ArrayHelper.replace(this.appClients, client, rename(client, name));
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
                    this.appClients = ArrayHelper.push(this.appClients, dto);
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

