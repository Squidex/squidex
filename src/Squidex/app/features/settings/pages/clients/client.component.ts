/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    AccessTokenDto,
    AppClientDto,
    AppClientsService,
    fadeAnimation,
    ModalView,
    NotificationService
} from 'shared';

const ESCAPE_KEY = 27;

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ClientComponent {
    public isRenaming = false;

    public token: AccessTokenDto;

    @Output()
    public renaming = new EventEmitter<string>();

    @Output()
    public revoking = new EventEmitter();

    @Output()
    public changing = new EventEmitter<boolean>();

    @Input()
    public appName: string;

    @Input()
    public client: AppClientDto;

    public tokenDialog = new ModalView();

    public get hasNewName() {
        return this.renameForm.controls['name'].value !== this.client.name;
    }

    public renameForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                Validators.required
            ]
        });

    constructor(
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: FormBuilder,
        private readonly notifications: NotificationService
    ) {
    }

    public cancelRename() {
        this.isRenaming = false;
    }

    public startRename() {
        this.renameForm.controls['name'].setValue(this.client.name);

        this.isRenaming = true;
    }

    public onKeyDown(keyCode: number) {
        if (keyCode === ESCAPE_KEY) {
            this.cancelRename();
        }
    }

    public createToken(client: AppClientDto) {
        this.appClientsService.createToken(this.appName, client)
            .subscribe(dto => {
                this.token = dto;
                this.tokenDialog.show();
            }, error => {
                this.notifications.notify(error);
            });
    }
}

