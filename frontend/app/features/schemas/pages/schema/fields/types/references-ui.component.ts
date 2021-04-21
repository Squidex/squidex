/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, REFERENCES_FIELD_EDITORS } from '@app/shared';

@Component({
    selector: 'sqx-references-ui',
    styleUrls: ['references-ui.component.scss'],
    templateUrl: 'references-ui.component.html'
})
export class ReferencesUIComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    public editors = REFERENCES_FIELD_EDITORS;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('editor',
                new FormControl(this.properties.editor, Validators.required));

            this.fieldForm.setControl('resolveReference',
                new FormControl());
        }

        this.fieldForm.patchValue(this.field.properties);
    }
}