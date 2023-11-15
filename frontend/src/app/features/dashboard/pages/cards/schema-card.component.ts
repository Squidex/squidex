/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
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
        NgIf,
        RouterLink,
        TranslatePipe,
    ],
})
export class SchemaCardComponent {
    @Input({ required: true })
    public app!: AppDto;
}
