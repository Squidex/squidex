/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html'
})
export class FieldFormCommonComponent {
    public readonly standalone = { standalone: true };

    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted = false;

    @Input()
    public field: FieldDto;
}