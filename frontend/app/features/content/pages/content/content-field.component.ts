/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppLanguageDto, AppsState, EditContentForm, fieldInvariant, invalid$, LocalStoreService, RootFieldDto, SchemaDto, StringFieldPropertiesDto, TranslationsService, Types, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { combineLatest } from 'rxjs/operators';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnChanges {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

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
    public languages: ReadonlyArray<AppLanguageDto>;

    public showAllControls = false;

    public isDifferent: Observable<boolean>;
    public isInvalid: Observable<boolean>;

    public get canTranslate() {
        const properties = this.field.properties;

        return this.field.isLocalizable && Types.is(properties, StringFieldPropertiesDto) && (properties.editor === 'Input' || properties.editor === 'TextArea');
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        this.showAllControls = this.localStore.getBoolean(this.configKey());

        if (changes['fieldForm'] && this.fieldForm) {
            this.isInvalid = invalid$(this.fieldForm);
        }

        if ((changes['fieldForm'] || changes['fieldFormCompare']) && this.fieldFormCompare) {
            this.isDifferent =
                value$(this.fieldForm).pipe(
                    combineLatest(value$(this.fieldFormCompare),
                        (lhs, rhs) => !Types.equals(lhs, rhs, true)));
        }
    }

    public changeShowAllControls(showAllControls: boolean) {
        this.showAllControls = showAllControls;

        this.localStore.setBoolean(this.configKey(), this.showAllControls);
    }

    public copy() {
        if (this.fieldFormCompare && this.fieldFormCompare) {
            if (this.showAllControls) {
                this.fieldForm.setValue(this.fieldFormCompare.value);
            } else {
                this.getControl()!.setValue(this.getControlCompare()!.value);
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
                    for (const language of this.languages) {
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
                const request = { text, sourceLanguage, targetLanguage };

                this.translations.translate(this.appsState.appName, request)
                    .subscribe(result => {
                        if (result.text) {
                            control.setValue(result.text);
                        }
                    });
            }
        }
    }

    private findControl(form?: FormGroup) {
        if (this.field.isLocalizable) {
            return form?.controls[this.language.iso2Code];
        } else {
            return form?.controls[fieldInvariant];
        }
    }

    public prefix(language: AppLanguageDto) {
        return `(${language.iso2Code})`;
    }

    public getControl() {
        return this.findControl(this.fieldForm);
    }

    public getControlCompare() {
        return this.findControl(this.fieldFormCompare);
    }

    public trackByLanguage(index: number, language: AppLanguageDto) {
        return language.iso2Code;
    }

    private configKey() {
        return `squidex.schemas.${this.schema?.id}.fields.${this.field?.fieldId}.show-all`;
    }
}