/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AuthService, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-notifications-menu',
    styleUrls: ['./notifications-menu.component.scss'],
    templateUrl: './notifications-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationsMenuComponent {
    public isNotifoConfigured = false;

    constructor(authService: AuthService, uiOptions: UIOptions,
    ) {
        const notifoApiKey = authService.user?.notifoToken;
        const notifoApiUrl = uiOptions.get('notifoApi');

        this.isNotifoConfigured = !!notifoApiKey && !!notifoApiUrl;
    }
}
