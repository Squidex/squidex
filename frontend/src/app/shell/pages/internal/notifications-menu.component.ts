/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AuthService, NotifoComponent, UIOptions } from '@app/shared';
import { NotificationDropdownComponent } from './notification-dropdown.component';

@Component({
    selector: 'sqx-notifications-menu',
    styleUrls: ['./notifications-menu.component.scss'],
    templateUrl: './notifications-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NotifoComponent,
        NgIf,
        NotificationDropdownComponent,
    ],
})
export class NotificationsMenuComponent {
    public isNotifoConfigured = false;

    constructor(authService: AuthService, uiOptions: UIOptions,
    ) {
        const notifoApiKey = authService.user?.notifoToken;
        const notifoApiUrl = uiOptions.value.notifoAPi;

        this.isNotifoConfigured = !!notifoApiKey && !!notifoApiUrl;
    }
}
