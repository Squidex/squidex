/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, HostBinding, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AppLanguageDto, AppsState, EditContentForm, FieldForm, invalid$, LocalStoreService, SchemaDto, Settings, TranslationsService, Types, value$ } from '@app/shared';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-content-field[form][formContext][formModel][language][languages][schema]',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
})
export class ContentFieldComponent implements OnChanges {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public isCompact?: boolean | null;

    @Input()
    public form: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

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

    @HostBinding('class')
    public get class() {
        return this.isHalfWidth ? 'col-6 half-field' : 'col-12';
    }

    public get isHalfWidth() {
        return this.formModel.field.properties.isHalfWidth && !this.isCompact && !this.formCompare;
    }

    public get isTranslatable() {
        return this.formModel.field.properties.fieldType === 'String' && this.formModel.field.isLocalizable && this.languages.length > 1;
    }

    public showAllControls = false;

    public isDifferent: Observable<boolean>;
    public isInvalid: Observable<boolean>;

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        this.showAllControls = this.localStore.getBoolean(this.configKey());

        if (changes['formModel'] && this.formModel) {
            this.isInvalid = invalid$(this.formModel.form);
        }

        if ((changes['formModel'] || changes['formModelCompare']) && this.formModelCompare) {
            this.isDifferent =
                combineLatest([
                    value$(this.formModel.form),
                    value$(this.formModelCompare!.form),
                ]).pipe(map(([lhs, rhs]) => !Types.equals(lhs, rhs, true)));
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
        return Settings.Local.FIELD_ALL(this.schema?.id, this.formModel.field.fieldId);
    }
}
