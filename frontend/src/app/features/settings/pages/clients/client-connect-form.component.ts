/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { KeyValuePipe } from '@angular/common';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { ApiUrlConfig, AppsState, ClientDto, ClientsService, ClientTourStated, CodeComponent, DialogService, ExternalLinkDirective, FormHintComponent, HelpService, MarkdownDirective, MessageBus, ModalDialogComponent, SDKEntry, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
    imports: [
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
    private readonly appsState = inject(AppsState);
    private readonly clientsService = inject(ClientsService);
    private readonly dialogs = inject(DialogService);
    private readonly helpService = inject(HelpService);
    private readonly messageBus = inject(MessageBus);

    protected readonly apiUrl = inject(ApiUrlConfig);

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** The client to show the connection instructions for. */
    public readonly client = input.required<ClientDto>();

    protected readonly sdk = signal<SDKEntry | undefined>(undefined);

    protected readonly step = signal('Start');

    protected readonly mcpServerName = computed(() => {
        return `squidex-${this.appName}`;
    });

    protected readonly mcpAuthorization = computed(() => {
        const client = this.client();

        // The client secret does not expire, unlike the access token.
        return `Basic ${btoa(`${this.appName}:${client.id}:${client.secret}`)}`;
    });

    protected readonly mcpUrl = computed(() => {
        return this.apiUrl.buildUrl(`/api/apps/${this.appName}/mcp`);
    });

    protected readonly mcpCommand = computed(() => {
        return `claude mcp add --transport http ${this.mcpServerName()} ${this.mcpUrl()} --header "Authorization: ${this.mcpAuthorization()}"`;
    });

    protected readonly mcpConfig = computed(() => {
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

    protected readonly appToken = rxResource({
        params: () => this.client(),
        stream: ({ params }) => this.clientsService.createToken(this.appName, params).pipe(
            catchError(error => {
                this.dialogs.notifyError(error);

                return of(undefined);
            })),
    });

    protected readonly sdks = rxResource({
        stream: () => this.helpService.getSDKs(),
    });

    protected readonly appName = this.appsState.appName;

    constructor() {
        this.messageBus.emit(new ClientTourStated());
    }

    protected select(sdk: SDKEntry) {
        const client = this.client();

        sdk.instructions = sdk.instructions.replace(/<APP_NAME>/g, this.appName);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_ID>/g, `${this.appName}:${client.id}`);
        sdk.instructions = sdk.instructions.replace(/<CLIENT_SECRET>/g, client.secret);
        sdk.instructions = sdk.instructions.replace(/<URL>/g, this.apiUrl.value);

        this.sdk.set(sdk);
    }

    protected go(step: string) {
        this.step.set(step);
    }
}
