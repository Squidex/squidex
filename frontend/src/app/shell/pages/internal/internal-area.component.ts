/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DialogService, LoadingService, Notification, ResourceOwner } from '@app/shared';

@Component({
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html',
})
export class InternalAreaComponent extends ResourceOwner implements OnInit {
    constructor(
        public readonly loadingService: LoadingService,
        private readonly dialogs: DialogService,
        private readonly route: ActivatedRoute,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
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
