/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto } from '@app/shared';

@Component({
    selector: 'sqx-schema-card[app]',
    styleUrls: ['./schema-card.component.scss'],
    templateUrl: './schema-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchemaCardComponent {
    @Input()
    public app!: AppDto;
}
