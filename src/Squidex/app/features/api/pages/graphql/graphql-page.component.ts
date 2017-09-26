/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs';

import * as React from 'react';
import * as ReactDOM from 'react-dom';

const GraphiQL = require('graphiql');

/* tslint:disable:use-view-encapsulation */

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    GraphQlService,
    LocalStoreService
} from 'shared';

@Component({
    selector: 'sqx-graphql-page',
    styleUrls: ['./graphql-page.component.scss'],
    templateUrl: './graphql-page.component.html',
    encapsulation: ViewEncapsulation.None
})
export class GraphQLPageComponent extends AppComponentBase implements OnInit {
    @ViewChild('graphiQLContainer')
    public graphiQLContainer: ElementRef;

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly graphQlService: GraphQlService,
        private readonly localStoreService: LocalStoreService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher: (params: any) => {
                    return this.request(params);
                },
                onEditQuery: (query: string) => {
                    this.localStoreService.set('graphiQlQuery', query);
                },
                query: this.localStoreService.get('graphiQlQuery')
            }),
            this.graphiQLContainer.nativeElement
        );
    }

    private request(params: any) {
        return this.appNameOnce()
            .switchMap(app => this.graphQlService.query(app, params).catch(response => Observable.of(response.error)))
            .toPromise();
    }
}

