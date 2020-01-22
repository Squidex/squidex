/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddLanguageForm,
    LanguageDto,
    LanguagesState
} from '@app/shared';

@Component({
    selector: 'sqx-language-add-form',
    template: `
        <div class="table-items-footer">
            <form [formGroup]="addLanguageForm.form" (ngSubmit)="addLanguage()">
                <div class="row no-gutters">
                    <div class="col">
                        <select class="form-control" formControlName="language">
                            <option *ngFor="let language of newLanguages" [ngValue]="language">{{language.englishName}}</option>
                        </select>
                    </div>
                    <div class="col-auto pl-1">
                        <button type="submit" class="btn btn-success">Add Language</button>
                    </div>
                </div>
            </form>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LanguageAddFormComponent implements OnChanges {
    @Input()
    public newLanguages: ReadonlyArray<LanguageDto>;

    public addLanguageForm = new AddLanguageForm(this.formBuilder);

    constructor(
        private readonly languagesState: LanguagesState,
        private readonly formBuilder: FormBuilder
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
                .subscribe(() => {
                    this.addLanguageForm.submitCompleted();
                }, error => {
                    this.addLanguageForm.submitFailed(error);
                });
        }
    }
}