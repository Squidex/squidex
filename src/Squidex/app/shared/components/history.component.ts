/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ImmutableArray, NotificationService } from 'framework';

import { AppComponentBase } from './../app-component-base';
import { AppsStoreService } from './../services/apps-store.service';
import { HistoryEventDto, HistoryService } from './../services/history.service';
import { UsersProviderService } from './../services/users-provider.service';

const FALLBACK_NAME = 'my-app';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent extends AppComponentBase implements OnDestroy, OnInit {
    private interval: any;

    public events = ImmutableArray.empty();

    constructor(appsStore: AppsStoreService, notifications: NotificationService, usersProvider: UsersProviderService,
        private readonly historyService: HistoryService,
        private readonly route: ActivatedRoute
    ) {
        super(appsStore, notifications, usersProvider);
    }

    public ngOnDestroy() {
        clearInterval(this.interval);
    }

    public ngOnInit() {
        this.load();

        this.interval =
            setInterval(() => {
                this.load();
            }, 10000);
    }

    public load() {
        const channel = this.route.snapshot.data['channel'];

        this.appName()
            .switchMap(app => this.historyService.getHistory(app, channel).retry(2))
            .subscribe(dtos => {
                this.events = ImmutableArray.of(dtos);
            });
    }
}