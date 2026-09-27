/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import { GraphiQL } from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { ApiUrlConfig, AppsState, AuthService, ClientDto, ClientsService, ClientsState, DialogModel, FormHintComponent, FormRowComponent, LayoutComponent, MessageBus, ModalDialogComponent, ModalDirective, QueryExecuted, Subscriptions, ThemeService, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';

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
export class GraphQLPageComponent implements AfterViewInit, OnInit, OnDestroy {
    private reactRoot?: ReactDOM.Root | null;

    @ViewChild('graphiQLContainer', { static: false })
    public graphiQLContainer!: ElementRef;

    private readonly subscriptions = new Subscriptions();
    private fetcher?: ReturnType<typeof createGraphiQLFetcher>;

    public clientsReadable = false;
    public clientsDialog = new DialogModel();
    public clientSelected: ClientDto | null = null;

    constructor(
        private readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
        private readonly clientsService: ClientsService,
        private readonly messageBus: MessageBus,
        private readonly themeService: ThemeService,
        public readonly clientsState: ClientsState,
    ) {
    }

    public ngOnDestroy() {
        this.reactRoot?.unmount();
        this.reactRoot = null;
    }

    public ngOnInit() {
        this.clientsReadable = this.appsState.snapshot.selectedApp!.canReadClients;

        if (this.clientsReadable) {
            this.clientsState.load();
        }
    }

    public ngAfterViewInit() {
        this.reactRoot = ReactDOM.createRoot(this.graphiQLContainer.nativeElement);
        this.selectClient(null);

        this.subscriptions.add(
            this.themeService.themeChanges
                .subscribe(() => {
                    this.render();
                }));
    }

    public selectClient(client: ClientDto | null) {
        this.clientSelected = client;

        if (!client) {
            this.initOrUpdateGraphQL(this.authService.user?.accessToken!);
        } else {
            this.clientsService.createToken(this.appsState.appName, client)
                .subscribe(token => {
                    if (this.clientSelected === client) {
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
