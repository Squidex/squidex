/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DialogService, LoadingService, Notification, Subscriptions, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html',
})
export class InternalAreaComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public readonly isEmbedded = inject(UIOptions).value.embedded;

    constructor(
        public readonly loadingService: LoadingService,
        private readonly dialogs: DialogService,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.route.queryParams.subscribe(params => {
                const successMessage = params['successMessage'];

                if (successMessage) {
                    this.dialogs.notify(Notification.info(successMessage));
                }

                const errorMessage = params['errorMessage'];

                if (errorMessage) {
                    this.dialogs.notify(Notification.error(errorMessage));
                }
            }));
    }
}
