/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, numberAttribute, output, signal } from '@angular/core';
import { AppLanguageDto, AppsState, CommentSelected, CommentsState, EditContentForm, FieldForm, FocusMarkerComponent, LocalStoreService, MenuItemComponent, MenuItemRegistry, MessageBus, SchemaDto, Settings, TranslateDto, TranslatePipe, TranslationsService, Types, UIOptions } from '@app/shared';
import { FieldCopyButtonComponent } from './field-copy-button.component';
import { FieldEditorComponent } from './field-editor.component';
import { FieldLanguagesComponent } from './field-languages.component';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FieldCopyButtonComponent,
        FieldEditorComponent,
        FieldLanguagesComponent,
        FocusMarkerComponent,
        MenuItemComponent,
        NgTemplateOutlet,
        TranslatePipe,
    ],
    providers: [MenuItemRegistry],
    host: {
        '[class]': 'hostClass()',
    },
})
export class ContentFieldComponent {
    private readonly appsState = inject(AppsState);
    private readonly localStore = inject(LocalStoreService);
    private readonly translations = inject(TranslationsService);
    private readonly hasTranslator = inject(UIOptions).value.canUseTranslator;

    private readonly isHalfWidth = computed(() => {
        return this.formModel().field.properties.isHalfWidth && !this.isCompact() && !this.formCompare();
    });

    private readonly isLocalizable = computed(() => {
        return this.formModel().field.isLocalizable;
    });

    /** Emits when the user selects another language. */
    public readonly languageChange = output<AppLanguageDto>();

    /** Renders the field with full width, even if it is configured as half width field. */
    public readonly isCompact = input(false, { transform: booleanAttribute });

    /** The content form the field belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** The content form of the other version, when comparing two versions. */
    public readonly formCompare = input<EditContentForm | null>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the field, 0 for root fields. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The form model of the field to edit. */
    public readonly formModel = input.required<FieldForm>();

    /** The form model of the field in the other version, when comparing two versions. */
    public readonly formModelCompare = input<FieldForm>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto>();

    /** The selected language. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly commentsState = inject(CommentsState, { optional: true });

    protected readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    protected readonly showAllControls = linkedSignal(() => {
        return this.localStore.getBoolean(this.showAllControlsKey());
    });

    protected readonly isCollapsed = signal(false);

    protected readonly isSelected = signal(false);

    protected readonly isInvalid = computed(() => {
        return this.formModel().invalid();
    });

    protected readonly isDisabled = computed(() => {
        return this.formModel().disabled();
    });

    protected readonly isDifferent = computed(() => {
        const formModelCompare = this.formModelCompare();
        if (!formModelCompare) {
            return false;
        }

        return !Types.equals(this.formModel().value(), formModelCompare.value(), true);
    });

    protected readonly isTranslatable = computed(() => {
        return this.formModel().field.properties.fieldType === 'String' && this.hasTranslator && this.isLocalizable() && this.languages().length > 1;
    });

    protected readonly control = computed(() => {
        return this.formModel().get(this.language().iso2Code);
    });

    protected readonly controlCompare = computed(() => {
        return this.formModelCompare()?.get(this.language().iso2Code);
    });

    protected readonly hostClass = computed(() => {
        return this.isHalfWidth() ? 'col-6 half-field' : 'col-12';
    });

    constructor() {
        const messageBus = inject(MessageBus);

        effect(onCleanup => {
            const subscription =
                messageBus.of(CommentSelected)
                    .subscribe(message => {
                        this.isSelected.set(message.editorId.indexOf(this.formModel().path) === 0);
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected changeShowAllControls(showAllControls: boolean) {
        this.showAllControls.set(showAllControls);

        this.localStore.setBoolean(this.showAllControlsKey(), showAllControls);
    }

    protected copy() {
        const formModel = this.formModel();
        const formModelCompare = this.formModelCompare();
        if (!formModelCompare) {
            return;
        }

        if (this.showAllControls()) {
            formModel.setValue(formModelCompare.form.value);
            return;
        }

        const languageCode = this.language().iso2Code;

        const target = formModel.get(languageCode);
        if (target) {
            target.setValue(formModelCompare.get(languageCode)?.form.value);
        }
    }

    protected translate() {
        const languages = this.languages();

        const master = languages.find(x => x.isMaster);
        if (!master) {
            return;
        }

        const masterCode = master.iso2Code;
        const masterValue = this.formModel().get(masterCode)!.form.value;
        if (!masterValue) {
            return;
        }

        if (this.showAllControls()) {
            for (const language of languages.filter(x => !x.isMaster)) {
                this.translateValue(masterValue, masterCode, language.iso2Code);
            }
        } else {
            this.translateValue(masterValue, masterCode, this.language().iso2Code);
        }
    }

    private translateValue(text: string, sourceLanguage: string, targetLanguage: string) {
        const control = this.formModel().get(targetLanguage);
        if (!control) {
            return;
        }

        if (control.form.value) {
            return;
        }

        const request = new TranslateDto({
            sourceLanguage,
            text,
            targetLanguage,
        });

        this.translations.translate(this.appsState.appName, request)
            .subscribe(result => {
                if (result.text) {
                    control.form.setValue(result.text);
                }
            });
    }

    protected prefix(language: AppLanguageDto) {
        return `(${language.iso2Code})`;
    }

    private showAllControlsKey() {
        return Settings.Local.FIELD_ALL(this.schema()?.id, this.formModel().field.fieldId);
    }

    protected toggle() {
        this.isCollapsed.update(x => !x);
    }
}
