/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import {
    AppsStoreService,
    AppClientDto,
    AppClientCreateDto,
    AppClientsService,
    Notification,
    NotificationService,
    TitleService 
} from 'shared';

@Ng2.Component({
    selector: 'sqx-clients-page',
    styles,
    template
})
export class ClientsPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;
    private appName: string | null = null;

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

    constructor(
        private readonly titles: TitleService,
        private readonly appsStore: AppsStoreService,
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: Ng2Forms.FormBuilder,
        private readonly notifications: NotificationService
    ) {
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.appName = app.name;

                    this.titles.setTitle('{appName} | Settings | Clients', { appName: app.name });

                    this.load();
                }
            });
    }

    public load() {
        this.appClientsService.getClients(this.appName)
            .subscribe(clients => {
                this.appClients = clients;
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
                this.appClients = [];
            });
    }

    public revokeClient(client: AppClientDto) {
        this.appClientsService.deleteClient(this.appName, client.id)
            .subscribe(() => {
                this.appClients.splice(this.appClients.indexOf(client), 1);
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public attachClient() {
        this.createForm.markAsDirty();

        if (this.createForm.valid) {
            this.createForm.disable();

            const dto = new AppClientCreateDto(this.createForm.controls['name'].value);

            this.appClientsService.postClient(this.appName, dto)
                .subscribe(client => {
                    this.appClients.push(client);
                    this.reset();
                }, error => {
                    this.notifications.notify(Notification.error(error.displayMessage));
                    this.reset();
                });
        }
    }

    private reset() {
        this.createForm.reset();
        this.createForm.enable();
    }
}

