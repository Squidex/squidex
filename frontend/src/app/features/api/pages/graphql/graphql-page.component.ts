/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Component, DestroyRef, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import { GraphiQL } from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { ApiUrlConfig, AppsState, AuthService, ClientDto, ClientsService, ClientsState, DialogModel, FormHintComponent, FormRowComponent, LayoutComponent, MessageBus, ModalDialogComponent, ModalDirective, QueryExecuted, ThemeService, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
    imports: [
        FormHintComponent,
        FormsModule,
        FormRowComponent,
        LayoutComponent,
        ModalDialogComponent,
        ModalDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class GraphQLPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly authService = inject(AuthService);
    private readonly clientsService = inject(ClientsService);
    private readonly messageBus = inject(MessageBus);
    private readonly themeService = inject(ThemeService);
    private readonly graphiQLContainer = viewChild.required<ElementRef<HTMLDivElement>>('graphiQLContainer');

    private reactRoot?: ReactDOM.Root | null;
    private fetcher?: ReturnType<typeof createGraphiQLFetcher>;

    protected readonly clientsState = inject(ClientsState);

    protected readonly clientsReadable: boolean;

    protected readonly clientsDialog = new DialogModel();

    protected readonly clientSelected = signal<ClientDto | null>(null);

    constructor() {
        const destroyRef = inject(DestroyRef);

        this.clientsReadable = this.appsState.snapshot.selectedApp!.canReadClients;

        if (this.clientsReadable) {
            this.clientsState.load();
        }

        afterNextRender(() => {
            this.reactRoot = ReactDOM.createRoot(this.graphiQLContainer().nativeElement);
            this.selectClient(null);
        });

        effect(() => {
            this.themeService.theme();
            this.render();
        });

        destroyRef.onDestroy(() => {
            this.reactRoot?.unmount();
            this.reactRoot = null;
        });
    }

    protected selectClient(client: ClientDto | null) {
        this.clientSelected.set(client);

        if (!client) {
            this.initOrUpdateGraphQL(this.authService.user?.accessToken!);
        } else {
            this.clientsService.createToken(this.appsState.appName, client)
                .subscribe(token => {
                    if (this.clientSelected() === client) {
                        this.initOrUpdateGraphQL(token.accessToken);
                    }
                });
        }
    }

    private initOrUpdateGraphQL(accessToken: string) {
        const graphQLEndpoint = this.apiUrl.buildUrl(`api/content/${this.appsState.appName}/graphql`);

        const subscriptionUrl =
            graphQLEndpoint
                .replace('http://', 'ws://')
                .replace('https://', 'wss://') +
                `?access_token=${accessToken}`;

        this.fetcher = createGraphiQLFetcher({
            url: graphQLEndpoint,
            headers: {
                Authorization: `Bearer ${accessToken}`,
            },
            fetch: (input: any, init?: RequestInit) => {
                const isIntrospection = Types.isString(init?.body) && init!.body.indexOf('IntrospectionQuery') >= 0;

                if (!isIntrospection) {
                    this.messageBus.emit(new QueryExecuted());
                }

                return fetch(input, init);
            },
            subscriptionUrl,
        });

        this.render();
    }

    private render() {
        if (!this.fetcher) {
            return;
        }

        this.reactRoot?.render(
            React.createElement(GraphiQL, {
                fetcher: this.fetcher,
                forcedTheme: this.themeService.theme(),
            }),
        );
    }
}
