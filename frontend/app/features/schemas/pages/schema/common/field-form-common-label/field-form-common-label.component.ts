/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { FieldDto, LanguageDto, StringFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common-label',
    templateUrl: './field-form-common.component.html'
})

export class FieldFormCommonLabelComponent implements OnInit {
    public readonly standalone = { standalone: true };

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: StringFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    public ngOnInit() {
        this.fieldForm.setControl('defaultValues',
            new FormControl(this.properties.defaultValues));
    }
}
