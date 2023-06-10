/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import GraphiQL from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { ApiUrlConfig, AppsState, AuthService, ClientDto, ClientsService, ClientsState, DialogModel } from '@app/shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
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
            subscriptionUrl,
        });

        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher,
            }),
            this.graphiQLContainer.nativeElement,
        );
    }
}
