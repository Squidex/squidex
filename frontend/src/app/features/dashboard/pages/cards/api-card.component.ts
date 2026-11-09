/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { AppDto, ExternalLinkDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-api-card',
    styleUrls: ['./api-card.component.scss'],
    templateUrl: './api-card.component.html',
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class ApiCardComponent {
    /** The app of the dashboard. */
    public readonly app = input.required<AppDto>();
}
