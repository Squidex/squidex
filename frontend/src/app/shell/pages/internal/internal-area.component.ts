/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, inject } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterOutlet } from '@angular/router';
import { DialogService, LoadingService, Notification, UIOptions } from '@app/shared';
import { AssetUploaderComponent } from '@app/shared/components/assets/asset-uploader.component';
import { AppsMenuComponent } from './apps-menu.component';
import { ChatMenuComponent } from './chat-menu.component';
import { LogoComponent } from './logo.component';
import { NotificationsMenuComponent } from './notifications-menu.component';
import { ProfileMenuComponent } from './profile-menu.component';
import { SearchMenuComponent } from './search-menu.component';

@Component({
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html',
    imports: [
        AppsMenuComponent,
        AssetUploaderComponent,
        ChatMenuComponent,
        LogoComponent,
        NotificationsMenuComponent,
        ProfileMenuComponent,
        RouterLink,
        RouterOutlet,
        SearchMenuComponent,
    ],
})
export class InternalAreaComponent {
    protected readonly isEmbedded = inject(UIOptions).value.embedded;

    constructor(
        protected readonly loadingService: LoadingService,
        private readonly dialogs: DialogService,
        private readonly route: ActivatedRoute,
    ) {
        effect(onCleanup => {
            const subscription =
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

            onCleanup(() => subscription.unsubscribe());
        });
    }
}
