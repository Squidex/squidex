/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AccessTokenDto,
    AppClientDto,
    AppClientsService,
    ComponentBase,
    DialogService,
    fadeAnimation,
    ModalView
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
export class ClientComponent extends ComponentBase {
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

    public isRenaming = false;

    public token: AccessTokenDto;
    public tokenDialog = new ModalView();

    public renameForm =
        this.formBuilder.group({
            name: ['',
                Validators.required
            ]
        });

    public get hasNewName() {
        return this.renameForm.controls['name'].value !== this.client.name;
    }

    constructor(dialogs: DialogService,
        private readonly appClientsService: AppClientsService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs);
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
                this.notifyError(error);
            });
    }
}

