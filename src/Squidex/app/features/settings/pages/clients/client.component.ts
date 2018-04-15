/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AccessTokenDto,
    AppClientDto,
    AppClientsService,
    AppsState,
    ClientsState,
    DialogService,
    ModalView,
    UpdateAppClientDto
} from '@app/shared';

const ESCAPE_KEY = 27;

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html'
})
export class ClientComponent {
    @Input()
    public client: AppClientDto;

    public clientPermissions = [ 'Developer', 'Editor', 'Reader' ];

    public isRenaming = false;

    public token: AccessTokenDto;
    public tokenDialog = new ModalView();

    public renameForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        });

    public get hasNewName() {
        return this.renameForm.controls['name'].value !== this.client.name;
    }

    constructor(
        public readonly appsState: AppsState,
        private readonly appClientsService: AppClientsService,
        private readonly clientsState: ClientsState,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public revoke() {
        this.clientsState.revoke(this.client).onErrorResumeNext().subscribe();
    }

    public updatePermission(permission: string) {
        this.clientsState.update(this.client, new UpdateAppClientDto(undefined, permission)).onErrorResumeNext().subscribe();
    }

    public rename() {
        this.clientsState.update(this.client, new UpdateAppClientDto(this.renameForm.controls['name'].value)).onErrorResumeNext().subscribe();
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
        this.appClientsService.createToken(this.appsState.appName, client)
            .subscribe(dto => {
                this.token = dto;
                this.tokenDialog.show();
            }, error => {
                this.dialogs.notifyError(error);
            });
    }
}

