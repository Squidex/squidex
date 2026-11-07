/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { afterNextRender, Component, DestroyRef, effect, ElementRef, signal, viewChild } from '@angular/core';
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
        AsyncPipe,
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
    private readonly graphiQLContainer = viewChild.required<ElementRef<HTMLDivElement>>('graphiQLContainer');
    private reactRoot?: ReactDOM.Root | null;
    private fetcher?: ReturnType<typeof createGraphiQLFetcher>;

    public readonly clientsReadable: boolean;
    public readonly clientsDialog = new DialogModel();
    public readonly clientSelected = signal<ClientDto | null>(null);

    constructor(
        private readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
        private readonly clientsService: ClientsService,
        private readonly messageBus: MessageBus,
        private readonly themeService: ThemeService,
        public readonly clientsState: ClientsState,
        destroyRef: DestroyRef,
    ) {
        this.clientsReadable = this.appsState.snapshot.selectedApp!.canReadClients;

        if (this.clientsReadable) {
            this.clientsState.load();
        }

        afterNextRender(() => {
            this.reactRoot = ReactDOM.createRoot(this.graphiQLContainer().nativeElement);
            this.selectClient(null);
        });

        effect(onCleanup => {
            const subscription =
                this.themeService.themeChanges
                    .subscribe(() => {
                        this.render();
                    });

            onCleanup(() => subscription.unsubscribe());
        });

        destroyRef.onDestroy(() => {
            this.reactRoot?.unmount();
            this.reactRoot = null;
        });
    }

    public selectClient(client: ClientDto | null) {
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
                forcedTheme: this.themeService.theme,
            }),
        );
    }
}
