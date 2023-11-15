/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TourStepDirective } from '@app/shared';
import { AppDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-settings-menu',
    styleUrls: ['./settings-menu.component.scss'],
    templateUrl: './settings-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        TourStepDirective,
        RouterLink,
        RouterLinkActive,
        TranslatePipe,
    ],
})
export class SettingsMenuComponent {
    @Input({ required: true })
    public app!: AppDto;
}
