/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-ui',
    styleUrls: ['field-form-ui.component.scss'],
    templateUrl: 'field-form-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FieldFormUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;
}