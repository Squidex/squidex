/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AddLanguageForm, AutocompleteSource, LanguageDto, LanguagesState } from '@app/shared';

class LanguageSource implements AutocompleteSource {
    constructor(
        private readonly languages: ReadonlyArray<LanguageDto>,
    ) {
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of(this.languages);
        }

        const regex = new RegExp(query, 'i');

        const results: LanguageDto[] = [];
        const result = this.languages.find(x => x.iso2Code === query);

        if (result) {
            results.push(result);
        }

        results.push(...this.languages.filter(x =>
            x.iso2Code !== query && (
            regex.test(x.iso2Code) ||
            regex.test(x.englishName))));

        return of(results);
    }
}

@Component({
    selector: 'sqx-language-add-form',
    styleUrls: ['./language-add-form.component.scss'],
    templateUrl: './language-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageAddFormComponent {
    @Input()
    public set newLanguages(value: ReadonlyArray<LanguageDto>) {
        this.addLanguagesSource = new LanguageSource(value);
    }

    public addLanguagesSource = new LanguageSource([]);
    public addLanguageForm = new AddLanguageForm();

    constructor(
        private readonly languagesState: LanguagesState,
    ) {
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
