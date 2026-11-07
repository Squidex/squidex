/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { AddLanguageForm, AutocompleteComponent, AutocompleteSource, FormHintComponent, LanguageDto, LanguagesState, StringHelper, TranslatePipe } from '@app/shared';

class LanguageSource implements AutocompleteSource {
    constructor(
        protected readonly languages: ReadonlyArray<LanguageDto>,
    ) {
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of(this.languages);
        }

        const regex = StringHelper.searchRegex(query);

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
    imports: [
        AutocompleteComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class LanguageAddFormComponent {
    private readonly languagesState = inject(LanguagesState);

    /** The languages that can still be added to the app. */
    public readonly newLanguages = input<ReadonlyArray<LanguageDto>>([]);

    protected readonly addLanguagesSource = computed(() => {
        return new LanguageSource(this.newLanguages());
    });

    protected readonly addLanguageForm = new AddLanguageForm();

    protected addLanguage() {
        const value = this.addLanguageForm.submit();
        if (!value) {
            return;
        }

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
