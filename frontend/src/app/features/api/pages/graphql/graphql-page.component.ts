/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import GraphiQL from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { ApiUrlConfig, AppsState, AuthService, ClientDto, ClientsService, ClientsState, DialogModel, FormHintComponent, LayoutComponent, MessageBus, ModalDialogComponent, ModalDirective, QueryExecuted, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
    imports: [
        AsyncPipe,
        FormHintComponent,
        FormsModule,
        LayoutComponent,
        ModalDialogComponent,
        ModalDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class GraphQLPageComponent implements AfterViewInit, OnInit {
    @ViewChild('graphiQLContainer', { static: false })
    public graphiQLContainer!: ElementRef;

    public clientsReadable = false;
    public clientsDialog = new DialogModel();
    public clientSelected: ClientDto | null = null;

    constructor(
        private readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
        private readonly clientsService: ClientsService,
        private readonly messageBus: MessageBus,
        public readonly clientsState: ClientsState,
    ) {
    }

    public ngOnInit() {
        this.clientsReadable = this.appsState.snapshot.selectedApp!.canReadClients;

        if (this.clientsReadable) {
            this.clientsState.load();
        }
    }

    public ngAfterViewInit() {
        this.selectClient(null);
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

        const fetcher = createGraphiQLFetcher({
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

        // eslint-disable-next-line deprecation/deprecation
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher,
            }),
            this.graphiQLContainer.nativeElement,
        );
    }
}
