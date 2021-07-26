/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AddLanguageForm, LanguageDto, LanguagesState } from '@app/shared';

@Component({
    selector: 'sqx-language-add-form',
    styleUrls: ['./language-add-form.component.scss'],
    templateUrl: './language-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageAddFormComponent implements OnChanges {
    @Input()
    public newLanguages: ReadonlyArray<LanguageDto>;

    public addLanguageForm = new AddLanguageForm(this.formBuilder);

    constructor(
        private readonly languagesState: LanguagesState,
        private readonly formBuilder: FormBuilder,
    ) {
    }

    public ngOnChanges() {
        if (this.newLanguages.length > 0) {
            const language = this.newLanguages[0];

            this.addLanguageForm.load({ language });
        }
    }

    public addLanguage() {
        const value = this.addLanguageForm.submit();

        if (value) {
            this.languagesState.add(value.language)
                .subscribe({
                    next: () => {
                        this.addLanguageForm.submitCompleted();
                    },
                    error: error => {
                        this.addLanguageForm.submitFailed(error);
                    },
                });
        }
    }
}
