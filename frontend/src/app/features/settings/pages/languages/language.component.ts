/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { AppLanguageDto, ConfirmClickDirective, EditLanguageForm, FormRowComponent, LanguageDto, LanguagesState, SignalSelectDirective, sorted, TranslatePipe, UpdateLanguageDto } from '@app/shared';

@Component({
    selector: 'sqx-language',
    styleUrls: ['./language.component.scss'],
    templateUrl: './language.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        ConfirmClickDirective,
        FormField,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class LanguageComponent {
    private readonly languagesState = inject(LanguagesState);

    /** The app language to show and edit. */
    public readonly language = input.required<AppLanguageDto>();

    /** The configured fallback languages of the language, in order. */
    public readonly fallbackLanguages = input.required<ReadonlyArray<LanguageDto>>();

    /** The languages that can still be added as fallback languages. */
    public readonly fallbackLanguagesNew = input.required<ReadonlyArray<LanguageDto>>();

    protected readonly fallbackLanguagesLocal = linkedSignal(() => this.fallbackLanguages());
    protected readonly fallbackLanguagesNewLocal = linkedSignal(() => this.fallbackLanguagesNew());
    protected readonly otherLanguageSelected = form(linkedSignal<LanguageDto | undefined>(() => this.fallbackLanguagesNewLocal()[0]));
    protected readonly isEditing = signal(false);

    protected readonly isEditable = computed(() => {
        return this.language().canUpdate;
    });

    protected readonly editForm = new EditLanguageForm();

    constructor() {
        effect(() => {
            this.editForm.load(this.language());
            this.editForm.setEnabled(this.isEditable());
        });
    }

    protected toggleEditing() {
        this.isEditing.update(x => !x);
    }

    protected remove() {
        this.languagesState.remove(this.language());
    }

    protected sort(event: CdkDragDrop<ReadonlyArray<AppLanguageDto>>) {
        this.fallbackLanguagesLocal.set(sorted(event) as any);
    }

    protected save() {
        if (!this.isEditable()) {
            return;
        }

        this.editForm.submit(value => {
            const request = new UpdateLanguageDto({ ...value, fallback: this.fallbackLanguagesLocal().map(x => x.iso2Code) });

            return this.languagesState.update(this.language(), request);
        });
    }

    protected removeFallbackLanguage(language: LanguageDto) {
        this.fallbackLanguagesLocal.update(x => x.removed(language));
        this.fallbackLanguagesNewLocal.update(x => [...x, language].sortByString(l => l.iso2Code));
    }

    protected addFallbackLanguage() {
        const otherLanguage = this.otherLanguageSelected().value();
        if (!otherLanguage) {
            return;
        }

        this.fallbackLanguagesLocal.update(x => [...x, otherLanguage].sortByString(l => l.iso2Code));
        this.fallbackLanguagesNewLocal.update(x => x.removed(otherLanguage));
    }
}
