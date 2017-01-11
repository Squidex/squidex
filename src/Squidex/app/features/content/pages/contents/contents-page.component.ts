/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import {
    AppComponentBase,
    AppsStoreService,
    NotificationService,
    SchemaDetailsDto,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html'
})
export class ContentsPageComponent extends AppComponentBase implements OnInit {
    public schema: SchemaDetailsDto;

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly route: ActivatedRoute
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.route.data.map(p => p['schema']).subscribe(schema => {
            this.schema = schema;
        });
    }
}

