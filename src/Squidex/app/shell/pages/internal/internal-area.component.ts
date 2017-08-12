/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { DialogService, Notification } from 'shared';

@Component({
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html'
})
export class InternalAreaComponent implements OnDestroy, OnInit {
    private queryParamsSubscription: Subscription;

    public notifications: Notification[] = [];

    constructor(
        private readonly dialogs: DialogService,
        private readonly route: ActivatedRoute
    ) {
    }

    public ngOnDestroy() {
        this.queryParamsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.queryParamsSubscription =
            this.route.queryParams.subscribe(params => {
                const successMessage = params['successMessage'];

                if (successMessage) {
                    this.dialogs.notify(Notification.info(successMessage));
                }

                const errorMessage = params['errorMessage'];

                if (errorMessage) {
                    this.dialogs.notify(Notification.error(errorMessage));
                }
            });
    }
 }