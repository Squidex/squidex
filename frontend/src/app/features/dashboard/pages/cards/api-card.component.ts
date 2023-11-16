/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto, ExternalLinkDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-api-card',
    styleUrls: ['./api-card.component.scss'],
    templateUrl: './api-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class ApiCardComponent {
    @Input({ required: true })
    public app!: AppDto;
}
