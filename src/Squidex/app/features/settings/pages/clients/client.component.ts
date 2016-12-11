/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import {
    AccessTokenDto,
    AppClientDto,
    AppClientsService,
    fadeAnimation,
    ModalView,
    Notification,
    NotificationService
} from 'shared';

const ESCAPE_KEY = 27;

@Ng2.Component({
    selector: 'sqx-client',
    styles,
    template,
    animations: [
        fadeAnimation
    ]
})
export class ClientComponent {
    public isRenaming = false;

    public appClientToken: AccessTokenDto;

    @Ng2.Output()
    public renamed = new Ng2.EventEmitter<string>();

    @Ng2.Input()
    public client: AppClientDto;

    @Ng2.Input()
    public appName: string;

    public modalDialog = new ModalView();

    public get clientName() {
        return this.client.name || this.client.id;
    }

    public get clientId() {
        return this.appName + ':' + this.client.id;
    }

    public get clientSecret() {
        return this.client.secret;
    }

    public renameForm =
        this.formBuilder.group({
            name: ['']
        });

    constructor(
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: Ng2Forms.FormBuilder,
        private readonly notifications: NotificationService
    ) {
    }

    public resetForm() {
        this.renameForm.controls['name'].setValue(this.clientName);
    }

    public cancelRename() {
        this.isRenaming = false;
    }

    public startRename() {
        this.resetForm();

        this.isRenaming = true;
    }

    public onKeyDown(keyCode: number) {
        if (keyCode === ESCAPE_KEY) {
            this.cancelRename();
        }
    }

    public rename() {
        try {
            const newName = this.renameForm.controls['name'].value;

            if (newName !== this.clientName) {
                this.renamed.emit();
            }
        } finally {
            this.isRenaming = false;
        }
    }

    public createToken(client: AppClientDto) {
        this.appClientsService.createToken(this.appName, client)
            .subscribe(token => {
                this.appClientToken = token;
                this.modalDialog.show();
            }, error => {
                this.notifications.notify(Notification.error('Failed to retrieve access token. Please retry.'));
            });
    }
}

