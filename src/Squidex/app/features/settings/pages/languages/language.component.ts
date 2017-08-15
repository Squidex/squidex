/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnChanges, OnDestroy, Output, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs';

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
export class LanguageComponent implements OnInit, OnChanges, OnDestroy {
    private isMasterSubscription: Subscription;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public allLanguages: ImmutableArray<AppLanguageDto>;

    @Output()
    public removing = new EventEmitter<AppLanguageDto>();

    @Output()
    public saving = new EventEmitter<AppLanguageDto>();

    public otherLanguages: ImmutableArray<AppLanguageDto>;
    public otherLanguage: AppLanguageDto;

    public fallbackLanguages: AppLanguageDto[] = [];

    public isEditing = false;
    public isMaster = false;

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            isMaster: [false, []],
            isOptional: [false, []]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.isMasterSubscription =
            this.editForm.controls['isMaster'].valueChanges
                .subscribe(v => {
                    this.isMaster = v;
                    this.editForm.controls['isOptional'].setValue(false);
                });

        this.resetEditForm();
    }

    public ngOnDestroy() {
        this.isMasterSubscription.unsubscribe();
    }

    public ngOnChanges() {
        this.resetEditForm();
    }

    public cancel() {
        this.resetEditForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public addLanguage() {
        this.addFallbackLanguage(this.otherLanguage);
    }

    public removeFallbackLanguage(language: AppLanguageDto) {
        this.fallbackLanguages.splice(this.fallbackLanguages.indexOf(language), 1);

        this.otherLanguages = this.otherLanguages.push(language);
        this.otherLanguage = this.otherLanguages.values[0];
    }

    public addFallbackLanguage(language: AppLanguageDto) {
        this.fallbackLanguages.push(language);

        this.otherLanguages = this.otherLanguages.filter(l => l.iso2Code !== language.iso2Code);
        this.otherLanguage = this.otherLanguages.values[0];
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const newLanguage =
                new AppLanguageDto(
                    this.language.iso2Code,
                    this.language.englishName,
                    this.editForm.controls['isMaster'].value,
                    this.editForm.controls['isOptional'].value,
                    this.fallbackLanguages.map(l => l.iso2Code));

            this.emitSaving(newLanguage);
        }
    }

    private emitSaving(language: AppLanguageDto) {
        this.saving.emit(language);
    }

    private resetEditForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.language);

        this.isEditing = false;

        if (this.language && this.allLanguages) {
            this.otherLanguages =
                this.allLanguages.filter(l =>
                    this.language.iso2Code !== l.iso2Code &&
                    this.language.fallback.indexOf(l.iso2Code) < 0);
            this.otherLanguage = this.otherLanguages.values[0];
        }

        if (this.language) {
            this.isMaster = this.language.isMaster;

            this.fallbackLanguages =
                this.allLanguages.filter(l =>
                    this.language.fallback.indexOf(l.iso2Code) >= 0).values;
        }
    }
}

