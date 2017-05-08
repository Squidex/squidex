/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnChanges, Output, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    AppLanguageDto,
    fadeAnimation,
    ImmutableArray
} from 'shared';

@Component({
    selector: 'sqx-language',
    styleUrls: ['./language.component.scss'],
    templateUrl: './language.component.html',
    animations: [
        fadeAnimation
    ]
})
export class LanguageComponent implements OnInit, OnChanges {
    @Input()
    public language: AppLanguageDto;

    @Input()
    public allLanguages: ImmutableArray<AppLanguageDto>;

    @Output()
    public removing = new EventEmitter<AppLanguageDto>();

    @Output()
    public saving = new EventEmitter<AppLanguageDto>();

    public otherLanguages: ImmutableArray<AppLanguageDto>;

    public fallbackLanguages: AppLanguageDto[] = [];

    public isEditing = false;

    public editFormSubmitted = false;
    public editForm: FormGroup =
        this.formBuilder.group({
            isMaster: [false, []],
            isOptional: [false, []]
        });

    public addLanguageForm: FormGroup =
        this.formBuilder.group({
            language: [null,
                Validators.required
            ]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.resetForm();
    }

    public ngOnChanges() {
        this.resetForm();
    }

    public cancel() {
        this.resetForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public addLanguage() {
        this.addFallbackLanguage(this.addLanguageForm.get('language')!.value);
    }

    public removeFallbackLanguage(language: AppLanguageDto) {
        this.fallbackLanguages.splice(this.fallbackLanguages.indexOf(language), 1);

        this.otherLanguages = this.otherLanguages.push(language);
    }

    public addFallbackLanguage(language: AppLanguageDto) {
        this.fallbackLanguages.push(language);

        this.otherLanguages = this.otherLanguages.filter(l => l.iso2Code !== language.iso2Code);
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const newLanguage =
                new AppLanguageDto(
                    this.language.iso2Code,
                    this.language.englishName,
                    this.editForm.get('isMaster')!.value,
                    this.editForm.get('isOptional')!.value,
                    this.fallbackLanguages.map(l => l.iso2Code));

            this.saving.emit(newLanguage);
        }
    }

    private resetForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.language);

        this.isEditing = false;

        if (this.language && this.allLanguages) {
            this.otherLanguages =
                this.allLanguages.filter(l =>
                    this.language.iso2Code !== l.iso2Code &&
                    this.language.fallback.indexOf(l.iso2Code) < 0);
        }

        if (this.language) {
            this.fallbackLanguages =
                this.allLanguages.filter(l =>
                    this.language.fallback.indexOf(l.iso2Code) >= 0).values;
        }
    }
}

