/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { BooleanFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html'
})
export class BooleanUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('placeholder',
            new FormControl(this.properties.placeholder, [
                Validators.maxLength(100)
            ]));

        this.editForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));
    }
}