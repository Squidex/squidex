/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';

import * as React from 'react';
import * as ReactDOM from 'react-dom';

const GraphiQL = require('graphiql');

import { AppsState, GraphQlService } from '@app/shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html'
})
export class GraphQLPageComponent implements OnInit {
    @ViewChild('graphiQLContainer')
    public graphiQLContainer: ElementRef;

    constructor(
        public readonly appsState: AppsState,
        private readonly graphQlService: GraphQlService
    ) {
    }

    public ngOnInit() {
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher: (params: any) => {
                    return this.request(params);
                }
            }),
            this.graphiQLContainer.nativeElement
        );
    }

    private request(params: any) {
        return this.graphQlService.query(this.appsState.appName, params).catch(response => Observable.of(response.error)).toPromise();
    }
}

