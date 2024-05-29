/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
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
    @Input({ required: true })
    public app!: AppDto;
}
