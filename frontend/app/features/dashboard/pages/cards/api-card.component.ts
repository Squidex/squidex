/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-api-card',
    styleUrls: ['./api-card.component.scss'],
    templateUrl: './api-card.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApiCardComponent {
    @Input()
    public app: AppDto;
}