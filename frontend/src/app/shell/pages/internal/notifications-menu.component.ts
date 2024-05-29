/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AuthService, NotifoComponent, UIOptions } from '@app/shared';
import { NotificationDropdownComponent } from './notification-dropdown.component';

@Component({
    standalone: true,
    selector: 'sqx-notifications-menu',
    styleUrls: ['./notifications-menu.component.scss'],
    templateUrl: './notifications-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NotificationDropdownComponent,
        NotifoComponent,
    ],
})
export class NotificationsMenuComponent {
    public isNotifoConfigured = false;

    constructor(authService: AuthService, uiOptions: UIOptions,
    ) {
        const notifoApiKey = authService.user?.notifoToken;
        const notifoApiUrl = uiOptions.value.notifoApi;

        this.isNotifoConfigured = !!notifoApiKey && !!notifoApiUrl;
    }
}
