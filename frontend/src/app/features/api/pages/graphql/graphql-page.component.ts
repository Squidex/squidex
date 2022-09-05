/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { createGraphiQLFetcher } from '@graphiql/toolkit';
import GraphiQL from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { ApiUrlConfig, AppsState, AuthService } from '@app/shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
})
export class GraphQLPageComponent implements AfterViewInit {
    @ViewChild('graphiQLContainer', { static: false })
    public graphiQLContainer!: ElementRef;

    constructor(
        private readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
    ) {
    }

    public ngAfterViewInit() {
        const url = this.apiUrl.buildUrl(`api/content/${this.appsState.appName}/graphql`);

        const subscriptionUrl =
            url
                .replace('http://', 'ws://')
                .replace('https://', 'wss://') +
            `?access_token=${this.authService.user?.accessToken}`;

        const fetcher = createGraphiQLFetcher({
            url,
            subscriptionUrl,
            headers: {
                Authorization: `Bearer ${this.authService.user?.accessToken}`,
            },
        });

        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher,
            }),
            this.graphiQLContainer.nativeElement,
        );
    }
}
