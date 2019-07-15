/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

import {
    AccessTokenDto,
    ApiUrlConfig,
    AppsState,
    ClientDto,
    ClientsService,
    ClientsState,
    DialogModel,
    DialogService,
    RoleDto
} from '@app/shared';

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html'
})
export class ClientComponent implements OnChanges {
    @Input()
    public client: ClientDto;

    @Input()
    public clientRoles: RoleDto[];

    public connectToken: AccessTokenDto;
    public connectDialog = new DialogModel();

    public connectHttpText: string;
    public connectCLINixText: string;
    public connectCLIWinText: string;
    public connectCLINetText: string;
    public connectLibraryText: string;

    constructor(
        public readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly clientsService: ClientsService,
        private readonly clientsState: ClientsState,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['client']) {
            const app = this.appsState.appName;

            this.connectHttpText = connectHttpText(this.apiUrl, app, this.client);
            this.connectCLINetText = connectCLINetText(app, this.client, this.apiUrl);
            this.connectCLINixText = connectCLINixText(app, this.client, this.apiUrl);
            this.connectCLIWinText = connectCLIWinText(app, this.client, this.apiUrl);
            this.connectLibraryText = connectLibrary(this.apiUrl, app, this.client);
        }
    }

    public revoke() {
        this.clientsState.revoke(this.client);
    }

    public update(role: string) {
        this.clientsState.update(this.client, { role });
    }

    public rename(name: string) {
        this.clientsState.update(this.client, { name });
    }

    public connect() {
        this.connectDialog.show();

        this.clientsService.createToken(this.appsState.appName, this.client)
            .subscribe(dto => {
                this.connectToken = dto;
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public trackByRole(index: number, role: RoleDto) {
        return role.name;
    }
}

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

function connectCLIWinText(app: string, client: { id: string, secret: string }, url: ApiUrlConfig) {
    return `.\\sq.exe config add ${app} ${app}:${client.id} ${client.secret} -u ${url.value};.\\sq.exe config use ${app}`;
}

function connectCLINixText(app: string, client: { id: string, secret: string }, url: ApiUrlConfig) {
    return `sq config add ${app} ${app}:${client.id} ${client.secret} -u ${url.value} && sq config use ${app}`;
}

function connectCLINetText(app: string, client: { id: string, secret: string }, url: ApiUrlConfig) {
    return `dotnet sq.dll config add ${app} ${app}:${client.id} ${client.secret} -u ${url.value}`;
}

function connectLibrary(apiUrl: ApiUrlConfig, app: string, client: { id: string, secret: string }) {
    const url = apiUrl.value;

    return `var clientManager = new SquidexClientManager(
    "${url}",
    "${app}",
    "${app}:${client.id}",
    "${client.secret}")`;
}