/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ConfirmClickDirective, EditLanguageForm, FormHintComponent, LanguageDto, LanguagesState, sorted, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-language',
    styleUrls: ['./language.component.scss'],
    templateUrl: './language.component.html',
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        ConfirmClickDirective,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class LanguageComponent {
    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public fallbackLanguages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public fallbackLanguagesNew!: ReadonlyArray<LanguageDto>;

    public otherLanguage!: LanguageDto;

    public isEditing?: boolean | null;
    public isEditable = false;

    public editForm = new EditLanguageForm();

    constructor(
        private readonly languagesState: LanguagesState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.language.canUpdate;

        this.editForm.load(this.language);
        this.editForm.setEnabled(this.isEditable);

        this.otherLanguage = this.fallbackLanguagesNew[0];
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public remove() {
        this.languagesState.remove(this.language);
    }

    public sort(event: CdkDragDrop<ReadonlyArray<AppLanguageDto>>) {
        this.fallbackLanguages = sorted(event);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            const request = { ...value, fallback: this.fallbackLanguages.map(x => x.iso2Code) };

            this.languagesState.update(this.language, request)
                .subscribe({
                    next: () => {
                        this.editForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.editForm.submitFailed(error);
                    },
                });
        }
    }

    public removeFallbackLanguage(language: LanguageDto) {
        this.fallbackLanguages = this.fallbackLanguages.removed(language);
        this.fallbackLanguagesNew = [...this.fallbackLanguagesNew, language].sortByString(x => x.iso2Code);

        this.otherLanguage = this.fallbackLanguagesNew[0];
    }

    public addFallbackLanguage() {
        this.fallbackLanguages = [...this.fallbackLanguages, this.otherLanguage].sortByString(x => x.iso2Code);
        this.fallbackLanguagesNew = this.fallbackLanguagesNew.removed(this.otherLanguage);

        this.otherLanguage = this.fallbackLanguagesNew[0];
    }
}
