/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-support-card[app]',
    styleUrls: ['./support-card.component.scss'],
    templateUrl: './support-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SupportCardComponent {
    @Input()
    public app: AppDto;
}
