/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['field-form-common.component.scss'],
    templateUrl: 'field-form-common.component.html'
})
export class FieldFormCommonComponent implements OnChanges, OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted = false;

    @Input()
    public showName = true;

    @Input()
    public field: FieldDto;

    public ngOnInit() {
        this.editForm.setControl('isRequired',
            new FormControl(this.field.properties.isRequired));

        this.editForm.setControl('isListField',
            new FormControl(this.field.properties.isListField));

        this.editForm.setControl('editorUrl',
            new FormControl(this.field.properties.editorUrl));

        this.editForm.setControl('hints',
            new FormControl(this.field.properties.hints));

        this.editForm.setControl('placeholder',
            new FormControl(this.field.properties.placeholder));

        this.editForm.setControl('label',
            new FormControl(this.field.properties.label));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.editForm.reset(this.field.properties);
        }
    }
}