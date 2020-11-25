/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { FieldDto, LanguageDto } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html'
})
export class FieldFormCommonComponent implements OnInit {
    public readonly standalone = { standalone: true };

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    public showDefaultValue: Observable<string>;

    public ngOnInit() {
        this.fieldForm.setControl('label',
            new FormControl(this.field.properties.label));

         this.fieldForm.setControl('defaultValuesLabel',
            new FormControl(this.field.properties.defaultValuesLabel));

        this.fieldForm.setControl('hints',
            new FormControl(this.field.properties.hints));

         this.fieldForm.setControl('defaultValuesHints',
            new FormControl(this.field.properties.defaultValuesHints));   
    }
}