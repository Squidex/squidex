/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TeamDto } from '@app/shared';

@Component({
    selector: 'sqx-left-menu[team]',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeftMenuComponent {
    @Input()
    public team!: TeamDto;
}
