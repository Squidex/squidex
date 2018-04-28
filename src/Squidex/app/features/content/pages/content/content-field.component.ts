/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';

import {
    AppLanguageDto,
    FieldDto,
    fieldInvariant,
    ImmutableArray
} from '@app/shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnChanges {
    @Input()
    public field: FieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Input()
    public contentFormSubmitted: boolean;

    public selectedFormControl: AbstractControl;

    public ngOnChanges(changes: SimpleChanges) {
        if (this.field.isLocalizable) {
            this.selectedFormControl = this.fieldForm.controls[this.language.iso2Code];
        } else {
            this.selectedFormControl = this.fieldForm.controls[fieldInvariant];
        }

        if (changes['language']) {
            this.selectedFormControl['_clearChangeFns']();
        }
    }
}

