/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AddLanguageForm,
    AppLanguageDto,
    AppsState,
    LanguagesState
} from '@app/shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent implements OnDestroy, OnInit {
    private newLanguagesSubscription: Subscription;

    public addLanguageForm = new AddLanguageForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly languagesState: LanguagesState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnDestroy() {
        this.newLanguagesSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.newLanguagesSubscription =
            this.languagesState.newLanguages
                .subscribe(languages => {
                    if (languages.length > 0) {
                        this.addLanguageForm.load({ language: languages.at(0) });
                    }
                });

        this.languagesState.load().onErrorResumeNext().subscribe();
    }

    public reload() {
        this.languagesState.load(true).onErrorResumeNext().subscribe();
    }

    public addLanguage() {
        const value = this.addLanguageForm.submit();

        if (value) {
            this.languagesState.add(value.language)
                .subscribe(() => {
                    this.addLanguageForm.submitCompleted();
                }, error => {
                    this.addLanguageForm.submitFailed(error);
                });
        }
    }

    public trackByLanguage(index: number, language: { language: AppLanguageDto }) {
        return language.language;
    }
}

