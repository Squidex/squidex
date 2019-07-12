/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppLanguageDto,
    EditLanguageForm,
    fadeAnimation,
    ImmutableArray,
    LanguagesState
} from '@app/shared';

@Component({
    selector: 'sqx-language',
    styleUrls: ['./language.component.scss'],
    templateUrl: './language.component.html',
    animations: [
        fadeAnimation
    ]
})
export class LanguageComponent implements OnChanges {
    @Input()
    public language: AppLanguageDto;

    @Input()
    public fallbackLanguages: ImmutableArray<AppLanguageDto>;

    @Input()
    public fallbackLanguagesNew: ImmutableArray<AppLanguageDto>;

    public otherLanguage: AppLanguageDto;

    public isEditing = false;
    public isEditable = false;

    public editForm = new EditLanguageForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly languagesState: LanguagesState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.language.canUpdate;

        this.editForm.load(this.language);
        this.editForm.setEnabled(this.isEditable);

        this.otherLanguage = this.fallbackLanguagesNew.at(0);
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public remove() {
        this.languagesState.remove(this.language);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            const request = { ...value, fallback: this.fallbackLanguages.map(x => x.iso2Code).values };

            this.languagesState.update(this.language, request)
                .subscribe(() => {
                    this.editForm.submitCompleted();

                    this.toggleEditing();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }

    public removeFallbackLanguage(language: AppLanguageDto) {
        this.fallbackLanguages = this.fallbackLanguages.remove(language);
        this.fallbackLanguagesNew = this.fallbackLanguagesNew.push(language).sortByStringAsc(x => x.iso2Code);

        this.otherLanguage = this.fallbackLanguagesNew.at(0);
    }

    public addFallbackLanguage() {
        this.fallbackLanguages = this.fallbackLanguages.push(this.otherLanguage);
        this.fallbackLanguagesNew = this.fallbackLanguagesNew.remove(this.otherLanguage);

        this.otherLanguage = this.fallbackLanguagesNew.at(0);
    }

    public trackByLanguage(index: number, language: AppLanguageDto) {
        return language.iso2Code;
    }
}

