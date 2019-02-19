/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { combineLatest } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    EditContentForm,
    fieldInvariant,
    invalid$,
    LocalStoreService,
    RootFieldDto,
    SchemaDto,
    TranslateDto,
    TranslationsService,
    Types,
    value$
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
    public isDifferent: Observable<boolean>;
    public isTranslatable: boolean;

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.showAllControls = this.localStore.getBoolean(this.configKey());
        }

        if (changes['fieldForm']) {
            this.isInvalid = invalid$(this.fieldForm);
        }

        if (changes['fieldForm'] || changes['field'] || changes['languages']) {
            this.isTranslatable = this.field.isTranslatable;
        }

        if ((changes['fieldForm'] || changes['fieldFormCompare']) && this.fieldFormCompare) {
            this.isDifferent =
                value$(this.fieldForm).pipe(
                    combineLatest(value$(this.fieldFormCompare),
                        (lhs, rhs) => !Types.jsJsonEquals(lhs, rhs)));
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

    public translate() {
        const master = this.languages.find(x => x.isMaster);

        if (master) {
            const masterCode = master.iso2Code;
            const masterValue = this.fieldForm.get(masterCode)!.value;

            if (masterValue) {
                if (this.showAllControls) {
                    for (let language of this.languages) {
                        if (!language.isMaster) {
                            this.translateValue(masterValue, masterCode, language.iso2Code);
                        }
                    }
                } else {
                    this.translateValue(masterValue, masterCode, this.language.iso2Code);
                }
            }
        }
    }

    private translateValue(text: string, sourceLanguage: string, targetLanguage: string) {
        const control = this.fieldForm.get(targetLanguage);

        if (control) {
            const value = control.value;

            if (!value) {
                const request = new TranslateDto(text, sourceLanguage, targetLanguage);

                this.translations.translate(this.appsState.appName, request)
                    .subscribe(result => {
                        if (result.text) {
                            control.setValue(result.text);
                        }
                    });
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
        return `(${language.iso2Code})`;
    }

    public trackByLanguage(index: number, language: AppLanguageDto) {
        return language.iso2Code;
    }

    private configKey() {
        return `squidex.schemas.${this.schema.id}.fields.${this.field.fieldId}.show-all`;
    }
}

