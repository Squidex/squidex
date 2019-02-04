/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

import {
    AppLanguageDto,
    EditContentForm,
    fieldInvariant,
    LocalStoreService,
    RootFieldDto,
    SchemaDto,
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
    public fieldFormCompare?: FormGroup;

    @Input()
    public schema: SchemaDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    public selectedFormControl: AbstractControl;
    public selectedFormControlCompare?: AbstractControl;

    public showAllControls = false;

    public isInvalid: Observable<boolean>;

    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.isInvalid = this.fieldForm.statusChanges.pipe(startWith(this.fieldForm.invalid), map(x => this.fieldForm.invalid));
        }

        if (changes['field']) {
            this.showAllControls = this.localStore.getBoolean(this.configKey());
        }

        const control = this.findControl(this.fieldForm);

        if (this.selectedFormControl !== control) {
            if (this.selectedFormControl && Types.isFunction(this.selectedFormControl['_clearChangeFns'])) {
                this.selectedFormControl['_clearChangeFns']();
            }

            this.selectedFormControl = control;
        }

        if (this.fieldFormCompare) {
            const controlCompare = this.findControl(this.fieldFormCompare);

            if (this.selectedFormControlCompare !== controlCompare) {
                if (this.selectedFormControlCompare && Types.isFunction(this.selectedFormControlCompare['_clearChangeFns'])) {
                    this.selectedFormControlCompare['_clearChangeFns']();
                }

                this.selectedFormControlCompare = controlCompare;
            }
        }
    }

    public changeShowAllControls(value: boolean) {
        this.showAllControls = value;

        this.localStore.setBoolean(this.configKey(), this.showAllControls);
    }

    public copy() {
        if (this.selectedFormControlCompare && this.fieldFormCompare) {
            if (this.showAllControls) {
                this.fieldForm.setValue(this.fieldFormCompare.value);
            } else {
                this.selectedFormControl.setValue(this.selectedFormControlCompare.value);
            }
        }
    }

    private findControl(form: FormGroup) {
        if (this.field.isLocalizable) {
            return form.controls[this.language.iso2Code];
        } else {
            return form.controls[fieldInvariant];
        }
    }

    public prefix(language: AppLanguageDto) {
        return `(${language.iso2Code}`;
    }

    public trackByLanguage(index: number, language: AppLanguageDto) {
        return language.iso2Code;
    }

    private configKey() {
        return `squidex.schemas.${this.schema.id}.fields.${this.field.fieldId}.show-all`;
    }
}

