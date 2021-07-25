/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AppLanguageDto, EditLanguageForm, LanguageDto, LanguagesState, sorted } from '@app/shared';

@Component({
    selector: 'sqx-language[language][fallbackLanguages][fallbackLanguagesNew]',
    styleUrls: ['./language.component.scss'],
    templateUrl: './language.component.html',
})
export class LanguageComponent implements OnChanges {
    @Input()
    public language: AppLanguageDto;

    @Input()
    public fallbackLanguages: ReadonlyArray<LanguageDto>;

    @Input()
    public fallbackLanguagesNew: ReadonlyArray<LanguageDto>;

    public otherLanguage: LanguageDto;

    public isEditing?: boolean | null;
    public isEditable = false;

    public editForm = new EditLanguageForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
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

    public trackByLanguage(_index: number, language: LanguageDto) {
        return language.iso2Code;
    }
}
