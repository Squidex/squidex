/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { BooleanFieldPropertiesDto, BOOLEAN_FIELD_EDITORS, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html'
})
export class BooleanUIComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public editors = BOOLEAN_FIELD_EDITORS;

    public ngOnInit() {
        this.fieldForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.fieldForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));
    }
}