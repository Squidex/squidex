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
    EditContentForm,
    fieldInvariant,
    ImmutableArray,
    RootFieldDto,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnChanges {
    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    public selectedFormControl: AbstractControl;

    public ngOnChanges(changes: SimpleChanges) {
        if (this.field.isLocalizable) {
            this.selectedFormControl = this.fieldForm.controls[this.language.iso2Code];
        } else {
            this.selectedFormControl = this.fieldForm.controls[fieldInvariant];
        }

        if (changes['language']) {
            if (Types.isFunction(this.selectedFormControl['_clearChangeFns'])) {
                this.selectedFormControl['_clearChangeFns']();
            }
        }
    }
}

