/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccessTokenDto, ApiUrlConfig, AppsState, ClientDto, ClientsService, DialogService, RoleDto } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
})
export class ClientConnectFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public client: ClientDto;

    @Input()
    public clientRoles: ReadonlyArray<RoleDto>;

    public appName: string;

    public connectToken: AccessTokenDto;
    public connectHttpText: string;
    public connectLibraryText: string;

    public step = 'Start';

    public get isStart() {
        return this.step === 'Start';
    }

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly clientsService: ClientsService,
        private readonly dialogs: DialogService,
    ) {
    }

    public ngOnInit() {
        this.appName = this.appsState.appName;

        this.connectHttpText = connectHttpText(this.apiUrl, this.appName, this.client);
        this.connectLibraryText = connectLibrary(this.apiUrl, this.appName, this.client);

        this.clientsService.createToken(this.appsState.appName, this.client)
            .subscribe({
                next: dto => {
                    this.connectToken = dto;

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    this.dialogs.notifyError(error);
                },
            });
    }

    public go(step: string) {
        this.step = step;
    }
}

function connectHttpText(apiUrl: ApiUrlConfig, app: string, client: { id: string; secret: string }) {
    const url = apiUrl.buildUrl('identity-server/connect/token');

    return `$ curl
    -X POST '${url}'
    -H 'Content-Type: application/x-www-form-urlencoded'
    -d 'grant_type=client_credentials&
        client_id=${app}:${client.id}&
        client_secret=${client.secret}&
        scope=squidex-api'`;
}

function connectLibrary(apiUrl: ApiUrlConfig, app: string, client: { id: string; secret: string }) {
    const url = apiUrl.value;

    return `var clientManager = new SquidexClientManager(
    "${url}",
    "${app}",
    "${app}:${client.id}",
    "${client.secret}")`;
}
