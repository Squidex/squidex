/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AppDto, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-settings-menu',
    styleUrls: ['./settings-menu.component.scss'],
    templateUrl: './settings-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SettingsMenuComponent {
    /** The app whose settings pages are listed in the menu. */
    public readonly app = input.required<AppDto>();
}
