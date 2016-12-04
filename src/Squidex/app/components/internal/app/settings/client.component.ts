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
    AppsStoreService,
    AppClientDto,
    AppClientCreateDto,
    AppClientsService,
    fadeAnimation,
    ModalView,
    Notification,
    NotificationService,
    TitleService 
} from 'shared';

@Ng2.Component({
    selector: 'sqx-client',
    styles,
    template,
    animations: [
        fadeAnimation
    ]
})
export class ClientComponent {
    private oldName: string;

    public isRenaming = false;

    public appClientToken: AccessTokenDto;

    @Ng2.Input('appName')
    public appName: string;

    @Ng2.Input('client')
    public client: AppClientDto;

    @Ng2.ViewChild('inputId')
    public inputId: Ng2.ElementRef;

    @Ng2.ViewChild('inputSecret')
    public inputSecret: Ng2.ElementRef;

    public modalDialog = new ModalView();

    constructor(
        private readonly appClientsService: AppClientsService,
        private readonly notifications: NotificationService
    ) {
    }

    public rename() {
        this.appClientsService.renameClient(this.appName, this.client.id, this.client.name)
            .subscribe(() => {
                this.stopRename();
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
                this.cancelRename();
            });
    }

    public cancelRename() {
        this.client.name = this.oldName;

        this.isRenaming = false;
    }

    public stopRename() {
        this.client.name = this.client.name || this.client.id;

        this.isRenaming = false;
    }

    public startRename() {
        this.oldName = this.client.name;

        this.isRenaming = true;
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

    public copyId() {
        this.copyToClipbord(this.inputId.nativeElement);
    }

    public copySecret() {
        this.copyToClipbord(this.inputSecret.nativeElement);
    }

    private copyToClipbord(element: HTMLInputElement | HTMLTextAreaElement) {
        const  currentFocus: any = document.activeElement;

        const prevSelectionStart = element.selectionStart;
        const prevSelectionEnd = element.selectionEnd;

        element.focus();
        element.setSelectionRange(0, element.value.length);
        
        try {
            document.execCommand('copy');
        } catch (e) {
            console.log('Copy failed');
        }

        if (currentFocus && typeof currentFocus.focus === 'function') {
            currentFocus.focus();
        }
        
        element.setSelectionRange(prevSelectionStart, prevSelectionEnd);
    }
}

