/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import * as React from 'react';
import * as ReactDOM from 'react-dom';

const GraphiQL = require('graphiql');

/* tslint:disable:use-view-encapsulation */

import {
    ApiUrlConfig,
    AppComponentBase,
    AppsStoreService,
    NotificationService
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

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly http: HttpClient
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        ReactDOM.render(
            React.createElement(GraphiQL, {
                fetcher: (params: any) => this.request(params)
            }),
            this.graphiQLContainer.nativeElement
        );
    }

    private request(params: any) {
        return this.appNameOnce()
            .switchMap(app => this.http.post(this.apiUrl.buildUrl(`api/content/${app}/graphql`), params))
            .toPromise();
    }
}

