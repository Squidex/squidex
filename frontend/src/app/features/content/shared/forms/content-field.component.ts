/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, HostBinding, inject, Input, numberAttribute, Optional, Output } from '@angular/core';
import { Observable } from 'rxjs';
import { AppLanguageDto, AppsState, changed$, CommentsState, disabled$, EditContentForm, FieldForm, FocusMarkerComponent, invalid$, LocalStoreService, SchemaDto, Settings, TooltipDirective, TranslationsService, TypedSimpleChanges, UIOptions } from '@app/shared';
import { FieldCopyButtonComponent } from './field-copy-button.component';
import { FieldEditorComponent } from './field-editor.component';
import { FieldLanguagesComponent } from './field-languages.component';

@Component({
    standalone: true,
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
    imports: [
        AsyncPipe,
        FieldCopyButtonComponent,
        FieldEditorComponent,
        FieldLanguagesComponent,
        FocusMarkerComponent,
        TooltipDirective,
    ],
})
export class ContentFieldComponent {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input({ transform: booleanAttribute })
    public isCompact?: boolean | null;

    @Input()
    public form!: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: FieldForm;

    @Input()
    public formModelCompare?: FieldForm;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    public showAllControls = false;

    public isDifferent?: Observable<boolean>;
    public isInvalid?: Observable<boolean>;
    public isDisabled?: Observable<boolean>;

    public readonly hasTranslator = inject(UIOptions).value.canUseTranslator;
    public readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    @HostBinding('class')
    public get class() {
        return this.isHalfWidth ? 'col-6 half-field' : 'col-12';
    }

    public get isHalfWidth() {
        return this.formModel.field.properties.isHalfWidth && !this.isCompact && !this.formCompare;
    }

    public get isTranslatable() {
        return this.formModel.field.properties.fieldType === 'String' && this.hasTranslator && this.formModel.field.isLocalizable && this.languages.length > 1;
    }

    constructor(
        @Optional() public readonly commentsState: CommentsState,
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly translations: TranslationsService,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        this.showAllControls = this.localStore.getBoolean(this.showAllControlsKey());

        if (changes.formModel && this.formModel) {
            this.isInvalid = invalid$(this.formModel.form);
            this.isDisabled = disabled$(this.formModel.form);
        }

        if ((changes.formModel || changes.formModelCompare) && this.formModelCompare) {
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

    private showAllControlsKey() {
        return Settings.Local.FIELD_ALL(this.schema?.id, this.formModel.field.fieldId);
    }
}
