/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService, NotifoComponent, UIOptions } from '@app/shared';
import { NotificationDropdownComponent } from './notification-dropdown.component';

@Component({
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
    private readonly authService = inject(AuthService);
    private readonly uiOptions = inject(UIOptions);

    protected readonly isNotifoConfigured = !!this.authService.user?.notifoToken && !!this.uiOptions.value.notifoApi;
}
