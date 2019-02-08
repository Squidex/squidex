/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AddLanguageForm,
    AppLanguageDto,
    AppsState,
    LanguagesState,
    ResourceOwner
} from '@app/shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent extends ResourceOwner implements OnInit {
    public addLanguageForm = new AddLanguageForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly languagesState: LanguagesState,
        private readonly formBuilder: FormBuilder
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.languagesState.newLanguages
                .subscribe(languages => {
                    if (languages.length > 0) {
                        this.addLanguageForm.load({ language: languages.at(0) });
                    }
                }));

        this.languagesState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.languagesState.load(true).pipe(onErrorResumeNext()).subscribe();
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

    public trackByLanguage(language: { language: AppLanguageDto }) {
        return language.language;
    }
}

