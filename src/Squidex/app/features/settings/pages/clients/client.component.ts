/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AccessTokenDto,
    ApiUrlConfig,
    AppClientDto,
    AppClientsService,
    AppRoleDto,
    AppsState,
    ClientsState,
    DialogModel,
    DialogService,
    RenameClientForm,
    UpdateAppClientDto
} from '@app/shared';

const ESCAPE_KEY = 27;

function connectHttpText(apiUrl: ApiUrlConfig, app: string, client: { id: string, secret: string }) {
    const url = apiUrl.buildUrl('identity-server/connect/token');

    return `$ curl
    -X POST '${url}'
    -H 'Content-Type: application/x-www-form-urlencoded'
    -d 'grant_type=client_credentials&
        client_id=${app}:${client.id}&
        client_secret=${client.secret}&
        scope=squidex-api`;
}

function connectCLIWinText(app: string, client: { id: string, secret: string }) {
    return `.\\sq.exe config add ${app} ${app}:${client.id} ${client.secret};.\\sq.exe config use ${app}`;
}

function connectCLINixText(app: string, client: { id: string, secret: string }) {
    return `sq config add ${app} ${app}:${client.id} ${client.secret} && sq config use ${app}`;
}

function connectLibrary(apiUrl: ApiUrlConfig, app: string, client: { id: string, secret: string }) {
    const url = apiUrl.value;

    return `var clientManager = new SquidexClientManager(
    "${url}",
    "${app}",
    "${app}:${client.id}",
    "${client.secret}")`;
}

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html'
})
export class ClientComponent implements OnChanges {
    @Input()
    public client: AppClientDto;

    @Input()
    public clientRoles: AppRoleDto[];

    public isRenaming = false;

    public connectToken: AccessTokenDto;
    public connectDialog = new DialogModel();

    public renameForm = new RenameClientForm(this.formBuilder);

    public connectHttpText: string;
    public connectCLINixText: string;
    public connectCLIWinText: string;
    public connectLibraryText: string;

    constructor(
        public readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly appClientsService: AppClientsService,
        private readonly clientsState: ClientsState,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges() {
        this.renameForm.load(this.client);

        const app = this.appsState.appName;

        this.connectHttpText = connectHttpText(this.apiUrl, app, this.client);
        this.connectCLINixText = connectCLINixText(app, this.client);
        this.connectCLIWinText = connectCLIWinText(app, this.client);
        this.connectLibraryText = connectLibrary(this.apiUrl, app, this.client);
    }

    public revoke() {
        this.clientsState.revoke(this.client).pipe(onErrorResumeNext()).subscribe();
    }

    public update(role: string) {
        this.clientsState.update(this.client, new UpdateAppClientDto(undefined, role)).pipe(onErrorResumeNext()).subscribe();
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

    public connect() {
        this.connectDialog.show();

        this.appClientsService.createToken(this.appsState.appName, this.client)
            .subscribe(dto => {
                this.connectToken = dto;
            }, error => {
                this.dialogs.notifyError(error);
            });
    }
}

