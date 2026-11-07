/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, KeyValuePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { AccessTokenDto, ApiUrlConfig, AppsState, ClientDto, ClientsService, ClientTourStated, CodeComponent, DialogService, ExternalLinkDirective, FormHintComponent, HelpService, MarkdownDirective, MessageBus, ModalDialogComponent, SDKEntry, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class ClientConnectFormComponent {
    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** The client to show the connection instructions for. */
    public readonly client = input.required<ClientDto>();

    public readonly sdks = this.helpService.getSDKs();
    public readonly sdk = signal<SDKEntry | undefined>(undefined);

    public readonly appName: string;
    public readonly appToken = signal<AccessTokenDto | undefined>(undefined);

    public readonly step = signal('Start');

    public readonly mcpUrl = computed(() => {
        return this.apiUrl.buildUrl(`/api/apps/${this.appName}/mcp`);
    });

    public readonly mcpServerName = computed(() => {
        return `squidex-${this.appName}`;
    });

    public readonly mcpCommand = computed(() => {
        return `claude mcp add --transport http ${this.mcpServerName()} ${this.mcpUrl()} --header "Authorization: ${this.mcpAuthorization()}"`;
    });

    public readonly mcpConfig = computed(() => {
        const config = {
            mcpServers: {
                [this.mcpServerName()]: {
                    type: 'http',
                    url: this.mcpUrl(),
                    headers: {
                        Authorization: this.mcpAuthorization(),
                    },
                },
            },
        };

        return JSON.stringify(config, undefined, 4);
    });

    private readonly mcpAuthorization = computed(() => {
        const client = this.client();

        // The client secret does not expire, unlike the access token.
        return `Basic ${btoa(`${this.appName}:${client.id}:${client.secret}`)}`;
    });

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        private readonly clientsService: ClientsService,
        private readonly dialogs: DialogService,
        private readonly helpService: HelpService,
        messageBus: MessageBus,
    ) {
        this.appName = appsState.appName;

        effect(onCleanup => {
            const subscription = this.clientsService.createToken(this.appName, this.client())
                .subscribe({
                    next: dto => {
                        this.appToken.set(dto);
                    },
                    error: error => {
                        this.dialogs.notifyError(error);
                    },
                });

            onCleanup(() => subscription.unsubscribe());
        });

        messageBus.emit(new ClientTourStated());
    }

    public select(sdk: SDKEntry) {
        const client = this.client();

        sdk.instructions = sdk.instructions.replace(/<APP_NAME>/g, this.appName);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_ID>/g, `${this.appName}:${client.id}`);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_SECRET>/g, client.secret);
        sdk.instructions = sdk.instructions.replace(/<URL>/g, this.apiUrl.value);

        this.sdk.set(sdk);
    }

    public go(step: string) {
        this.step.set(step);
    }
}
