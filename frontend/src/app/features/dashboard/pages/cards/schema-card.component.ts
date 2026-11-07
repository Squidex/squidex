/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-card',
    styleUrls: ['./schema-card.component.scss'],
    templateUrl: './schema-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RouterLink,
        TranslatePipe,
    ],
})
export class SchemaCardComponent {
    /** The app of the dashboard. */
    public readonly app = input.required<AppDto>();
}
