/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AccessTokenDto,
    AppClientDto,
    AppClientsService,
    AppsState,
    ClientsState,
    DialogService,
    ModalView,
    RenameClientForm,
    UpdateAppClientDto
} from '@app/shared';

const ESCAPE_KEY = 27;

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html'
})
export class ClientComponent implements OnChanges {
    @Input()
    public client: AppClientDto;

    public clientPermissions = [ 'Developer', 'Editor', 'Reader' ];

    public isRenaming = false;

    public token: AccessTokenDto;
    public tokenDialog = new ModalView();

    public renameForm = new RenameClientForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        private readonly appClientsService: AppClientsService,
        private readonly clientsState: ClientsState,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges() {
        this.renameForm.load(this.client);
    }

    public revoke() {
        this.clientsState.revoke(this.client).onErrorResumeNext().subscribe();
    }

    public update(permission: string) {
        this.clientsState.update(this.client, new UpdateAppClientDto(undefined, permission)).onErrorResumeNext().subscribe();
    }

    public toggleRename() {
        this.isRenaming = !this.isRenaming;
    }

    public onKeyDown(keyCode: number) {
        if (keyCode === ESCAPE_KEY) {
            this.toggleRename();
        }
    }

    public rename() {
        const value = this.renameForm.submit();

        if (value) {
            const requestDto = new UpdateAppClientDto(value.name);

            this.clientsState.update(this.client, requestDto)
                .subscribe(() => {
                    this.renameForm.submitCompleted();

                    this.toggleRename();
                }, error => {
                    this.renameForm.submitFailed(error);
                });
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

