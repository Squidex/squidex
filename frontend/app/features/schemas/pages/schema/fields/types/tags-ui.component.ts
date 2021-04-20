/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FieldDto, TagsFieldPropertiesDto, TAGS_FIELD_EDITORS } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html'
})
export class TagsUIComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: TagsFieldPropertiesDto;

    public editors = TAGS_FIELD_EDITORS;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('editor',
                new FormControl(undefined, Validators.required));

            this.fieldForm.setControl('allowedValues',
                new FormControl());
        }

        this.fieldForm.patchValue(this.field.properties);
    }
}