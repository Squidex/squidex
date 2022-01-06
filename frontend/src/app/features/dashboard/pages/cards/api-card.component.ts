/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto } from '@app/shared';

@Component({
    selector: 'sqx-api-card[app]',
    styleUrls: ['./api-card.component.scss'],
    templateUrl: './api-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiCardComponent {
    @Input()
    public app!: AppDto;
}
