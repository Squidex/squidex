/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TeamDto } from '@app/shared';

@Component({
    selector: 'sqx-settings-menu',
    styleUrls: ['./settings-menu.component.scss'],
    templateUrl: './settings-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsMenuComponent {
    @Input({ required: true })
    public team!: TeamDto;
}
