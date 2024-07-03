/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterOutlet } from '@angular/router';
import { DialogService, LoadingService, Notification, Subscriptions, UIOptions } from '@app/shared';
import { AssetUploaderComponent } from '@app/shared/components/assets/asset-uploader.component';
import { AppsMenuComponent } from './apps-menu.component';
import { ChatMenuComponent } from './chat-menu.component';
import { FeedbackMenuComponent } from './feedback-menu.component';
import { LogoComponent } from './logo.component';
import { NotificationsMenuComponent } from './notifications-menu.component';
import { ProfileMenuComponent } from './profile-menu.component';
import { SearchMenuComponent } from './search-menu.component';

@Component({
    standalone: true,
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html',
    imports: [
        AppsMenuComponent,
        AssetUploaderComponent,
        AsyncPipe,
        ChatMenuComponent,
        FeedbackMenuComponent,
        LogoComponent,
        NotificationsMenuComponent,
        ProfileMenuComponent,
        RouterLink,
        RouterOutlet,
        SearchMenuComponent,
    ],
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
