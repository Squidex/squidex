/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AppLanguageDto, AppsState, EditContentForm, FieldForm, invalid$, LocalStoreService, SchemaDto, StringFieldPropertiesDto, TranslationsService, Types, value$ } from '@app/shared';
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
    public formCompare?: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: FieldForm;

    @Input()
    public formModelCompare?: FieldForm;

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
        if (this.languages.length <= 1) {
            return false;
        }

        if (!this.formModel.field.isLocalizable) {
            return false;
        }

        const properties = this.formModel.field.properties;

        return Types.is(properties, StringFieldPropertiesDto) && (properties.editor === 'Input' || properties.editor === 'TextArea');
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        this.showAllControls = this.localStore.getBoolean(this.configKey());

        if (changes['formModel'] && this.formModel) {
            this.isInvalid = invalid$(this.formModel.form);
        }

        if ((changes['formModel'] || changes['formModelCompare']) && this.formModelCompare) {
            this.isDifferent =
                value$(this.formModel.form).pipe(
                    combineLatest(value$(this.formModelCompare!.form),
                        (lhs, rhs) => !Types.equals(lhs, rhs, true)));
        }
    }

    public changeShowAllControls(showAllControls: boolean) {
        this.showAllControls = showAllControls;

        this.localStore.setBoolean(this.configKey(), this.showAllControls);
    }

    public copy() {
        if (this.formModel && this.formModelCompare) {
            if (this.showAllControls) {
                this.formModel.copyAllFrom(this.formModelCompare);
            } else {
                this.formModel.copyFrom(this.formModelCompare, this.language.iso2Code);
            }
        }
    }

    public translate() {
        const master = this.languages.find(x => x.isMaster);

        if (master) {
            const masterCode = master.iso2Code;
            const masterValue = this.formModel.get(masterCode)!.form.value;

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
        const control = this.formModel.get(targetLanguage);

        if (control) {
            const value = control.form.value;

            if (!value) {
                const request = { text, sourceLanguage, targetLanguage };

                this.translations.translate(this.appsState.appName, request)
                    .subscribe(result => {
                        if (result.text) {
                            control.form.setValue(result.text);
                        }
                    });
            }
        }
    }

    public prefix(language: AppLanguageDto) {
        return `(${language.iso2Code})`;
    }

    public getControl() {
        return this.formModel.get(this.language.iso2Code);
    }

    public getControlCompare() {
        return this.formModelCompare?.get(this.language.iso2Code);
    }

    public trackByLanguage(_index: number, language: AppLanguageDto) {
        return language.iso2Code;
    }

    private configKey() {
        return `squidex.schemas.${this.schema?.id}.fields.${this.formModel.field.fieldId}.show-all`;
    }
}