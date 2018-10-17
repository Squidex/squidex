/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, DoCheck, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

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
export class ContentFieldComponent implements DoCheck, OnChanges {
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

    public isInvalid: Observable<boolean>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.isInvalid = this.fieldForm.statusChanges.pipe(startWith(this.fieldForm.invalid), map(x => this.fieldForm.invalid));
        }
    }

    public ngDoCheck() {
        let control: AbstractControl;

        if (this.field.isLocalizable) {
            control = this.fieldForm.controls[this.language.iso2Code];
        } else {
            control = this.fieldForm.controls[fieldInvariant];
        }

        if (this.selectedFormControl !== control) {
            if (this.selectedFormControl && Types.isFunction(this.selectedFormControl['_clearChangeFns'])) {
                this.selectedFormControl['_clearChangeFns']();
            }

            this.selectedFormControl = control;
        }
    }
}

