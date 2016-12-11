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
    fadeAnimation,
    ModalView
} from 'shared';

@Ng2.Component({
    selector: 'sqx-client',
    styles,
    template,
    animations: [
        fadeAnimation
    ]
})
export class ClientComponent implements Ng2.OnChanges {
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
        private readonly formBuilder: Ng2Forms.FormBuilder
    ) {
    }

    public ngOnChanges() {
        this.renameForm.controls['name'].setValue(this.clientName);
    }

    public cancelRename() {
        this.isRenaming = false;
    }

    public startRename() {
        this.isRenaming = true;
    }

    public rename() {
        try {
            this.renamed.emit(this.renameForm.controls['name'].value);
        } finally {
            this.isRenaming = false;
        }
    }
}

