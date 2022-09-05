/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, HostBinding, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { Observable } from 'rxjs';
import { AppLanguageDto, AppsState, changed$, disabled$, EditContentForm, FieldForm, invalid$, LocalStoreService, SchemaDto, Settings, TranslationsService } from '@app/shared';

@Component({
    selector: 'sqx-content-field[form][formContext][formLevel][formModel][language][languages][schema]',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
})
export class ContentFieldComponent implements OnChanges {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public isCompact?: boolean | null;

    @Input()
    public form!: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

    @Input()
    public formContext!: any;

    @Input()
    public formLevel!: number;

    @Input()
    public formModel!: FieldForm;

    @Input()
    public formModelCompare?: FieldForm;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    public showAllControls = false;

    public isDifferent?: Observable<boolean>;
    public isInvalid?: Observable<boolean>;
    public isDisabled?: Observable<boolean>;

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

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        this.showAllControls = this.localStore.getBoolean(this.showAllControlsKey());

        if (changes['formModel'] && this.formModel) {
            this.isInvalid = invalid$(this.formModel.form);
            this.isDisabled = disabled$(this.formModel.form);
        }

        if ((changes['formModel'] || changes['formModelCompare']) && this.formModelCompare) {
            this.isDifferent = changed$(this.formModel.form, this.formModelCompare.form);
        }
    }

    public changeShowAllControls(showAllControls: boolean) {
        this.showAllControls = showAllControls;

        this.localStore.setBoolean(this.showAllControlsKey(), this.showAllControls);
    }

    public copy() {
        if (this.formModel && this.formModelCompare) {
            if (this.showAllControls) {
                this.formModel.setValue(this.formModelCompare.form.value);
            } else {
                const target = this.formModel.get(this.language.iso2Code);

                if (target) {
                    target.setValue(this.formModelCompare.get(this.language.iso2Code)?.form.value);
                }
            }
        }
    }

    public translate() {
        const master = this.languages.find(x => x.isMaster);

        if (!master) {
            return;
        }

        const masterCode = master.iso2Code;
        const masterValue = this.formModel.get(masterCode)!.form.value;

        if (!masterValue) {
            return;
        }

        if (this.showAllControls) {
            for (const language of this.languages.filter(x => !x.isMaster)) {
                this.translateValue(masterValue, masterCode, language.iso2Code);
            }
        } else {
            this.translateValue(masterValue, masterCode, this.language.iso2Code);
        }
    }

    private translateValue(text: string, sourceLanguage: string, targetLanguage: string) {
        const control = this.formModel.get(targetLanguage);

        if (!control) {
            return;
        }

        if (control.form.value) {
            return;
        }

        const request = { text, sourceLanguage, targetLanguage };

        this.translations.translate(this.appsState.appName, request)
            .subscribe(result => {
                if (result.text) {
                    control.form.setValue(result.text);
                }
            });
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

    private showAllControlsKey() {
        return Settings.Local.FIELD_ALL(this.schema?.id, this.formModel.field.fieldId);
    }
}
