/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import GraphiQL from 'graphiql';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppsState, GraphQlService } from '@app/shared';

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
        private readonly graphQlService: GraphQlService,
    ) {
    }

    public ngAfterViewInit() {
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher: (params: any) => {
                    return firstValueFrom(this.request(params));
                },
            }),
            this.graphiQLContainer.nativeElement,
        );
    }

    private request(params: any) {
        return this.graphQlService.query(this.appsState.appName, params).pipe(
                catchError(response => of(response.error)));
    }
}
