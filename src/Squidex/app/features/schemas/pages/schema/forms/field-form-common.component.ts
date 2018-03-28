/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['field-form-common.component.scss'],
    templateUrl: 'field-form-common.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FieldFormCommonComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted = false;

    @Input()
    public showName = true;

    @Input()
    public field: FieldDto;

    public ngOnInit() {
        this.editForm.setControl('label',
            new FormControl(this.field.properties.label,
                Validators.maxLength(100)));

        this.editForm.setControl('hints',
            new FormControl(this.field.properties.label,
                Validators.maxLength(100)));

        this.editForm.setControl('isRequired',
            new FormControl(this.field.properties.isRequired));

        this.editForm.setControl('isListField',
            new FormControl(this.field.properties.isListField));
    }
}