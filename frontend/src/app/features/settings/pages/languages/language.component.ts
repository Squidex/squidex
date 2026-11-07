/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { ChangeDetectionStrategy, Component, computed, effect, input, linkedSignal, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ConfirmClickDirective, EditLanguageForm, FormRowComponent, LanguageDto, LanguagesState, sorted, TranslatePipe, UpdateLanguageDto } from '@app/shared';

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
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class LanguageComponent {
    /** The app language to show and edit. */
    public readonly language = input.required<AppLanguageDto>();

    /** The configured fallback languages of the language, in order. */
    public readonly fallbackLanguages = input.required<ReadonlyArray<LanguageDto>>();

    /** The languages that can still be added as fallback languages. */
    public readonly fallbackLanguagesNew = input.required<ReadonlyArray<LanguageDto>>();

    public readonly fallbackLanguagesLocal = linkedSignal(() => this.fallbackLanguages());

    public readonly fallbackLanguagesNewLocal = linkedSignal(() => this.fallbackLanguagesNew());

    public readonly otherLanguageSelected = linkedSignal<LanguageDto | undefined>(() => this.fallbackLanguagesNewLocal()[0]);

    public readonly isEditing = signal(false);

    public readonly isEditable = computed(() => {
        return this.language().canUpdate;
    });

    public readonly editForm = new EditLanguageForm();

    constructor(
        private readonly languagesState: LanguagesState,
    ) {
        effect(() => {
            this.editForm.load(this.language());
            this.editForm.setEnabled(this.isEditable());
        });
    }

    public toggleEditing() {
        this.isEditing.update(x => !x);
    }

    public remove() {
        this.languagesState.remove(this.language());
    }

    public sort(event: CdkDragDrop<ReadonlyArray<AppLanguageDto>>) {
        this.fallbackLanguagesLocal.set(sorted(event) as any);
    }

    public save() {
        if (!this.isEditable()) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        const request = new UpdateLanguageDto({ ...value, fallback: this.fallbackLanguagesLocal().map(x => x.iso2Code) });

        this.languagesState.update(this.language(), request)
            .subscribe({
                next: () => {
                    this.editForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }

    public removeFallbackLanguage(language: LanguageDto) {
        this.fallbackLanguagesLocal.update(x => x.removed(language));
        this.fallbackLanguagesNewLocal.update(x => [...x, language].sortByString(l => l.iso2Code));
    }

    public addFallbackLanguage() {
        const otherLanguage = this.otherLanguageSelected();
        if (!otherLanguage) {
            return;
        }

        this.fallbackLanguagesLocal.update(x => [...x, otherLanguage].sortByString(l => l.iso2Code));
        this.fallbackLanguagesNewLocal.update(x => x.removed(otherLanguage));
    }
}
