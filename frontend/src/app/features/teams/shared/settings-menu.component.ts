/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TeamDto, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-settings-menu',
    styleUrls: ['./settings-menu.component.scss'],
    templateUrl: './settings-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgIf,
        RouterLink,
        RouterLinkActive,
        TranslatePipe,
    ],
})
export class SettingsMenuComponent {
    @Input({ required: true })
    public team!: TeamDto;
}
