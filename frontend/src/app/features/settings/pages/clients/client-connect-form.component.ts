/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, KeyValuePipe } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccessTokenDto, ApiUrlConfig, AppsState, ClientDto, ClientsService, ClientTourStated, CodeComponent, DialogService, ExternalLinkDirective, FormHintComponent, HelpService, MarkdownDirective, MessageBus, ModalDialogComponent, SDKEntry, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
    imports: [
        AsyncPipe,
        CodeComponent,
        ExternalLinkDirective,
        FormHintComponent,
        KeyValuePipe,
        MarkdownDirective,
        ModalDialogComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ClientConnectFormComponent implements OnInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Input({ required: true })
    public client!: ClientDto;

    public sdks = this.helpService.getSDKs();
    public sdk?: SDKEntry;

    public appName!: string;
    public appToken?: AccessTokenDto;

    public step = 'Start';

    public get mcpUrl() {
        return this.apiUrl.buildUrl(`/api/apps/${this.appName}/mcp`);
    }

    public get mcpServerName() {
        return `squidex-${this.appName}`;
    }

    public get mcpCommand() {
        return `claude mcp add --transport http ${this.mcpServerName} ${this.mcpUrl} --header "Authorization: Bearer ${this.token}"`;
    }

    public get mcpConfig() {
        const config = {
            mcpServers: {
                [this.mcpServerName]: {
                    type: 'http',
                    url: this.mcpUrl,
                    headers: {
                        Authorization: `Bearer ${this.token}`,
                    },
                },
            },
        };

        return JSON.stringify(config, undefined, 4);
    }

    private get token() {
        return this.appToken?.accessToken || '[YOUR_TOKEN]';
    }

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly clientsService: ClientsService,
        private readonly dialogs: DialogService,
        private readonly helpService: HelpService,
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnInit() {
        this.appName = this.appsState.appName;

        this.clientsService.createToken(this.appsState.appName, this.client)
            .subscribe({
                next: dto => {
                    this.appToken = dto;

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    this.dialogs.notifyError(error);
                },
            });

        this.messageBus.emit(new ClientTourStated());
    }

    public select(sdk: SDKEntry) {
        sdk.instructions = sdk.instructions.replace(/<APP_NAME>/g, this.appName);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_ID>/g, `${this.appName}:${this.client.id}`);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_SECRET>/g, this.client.secret);
        sdk.instructions = sdk.instructions.replace(/<URL>/g, this.apiUrl.value);

        this.sdk = sdk;
    }

    public go(step: string) {
        this.step = step;
    }
}
