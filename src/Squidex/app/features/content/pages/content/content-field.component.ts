/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';

import {
    AppLanguageDto,
    FieldDto,
    fieldInvariant,
    ImmutableArray
} from '@app/shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements OnInit {
    @Input()
    public field: FieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Input()
    public contentFormSubmitted: boolean;

    public selectedFormControl: AbstractControl;

    public ngOnInit() {
        if (!this.language) {
            this.language = this.languages[0];
        }

        if (this.field.isLocalizable) {
            this.selectedFormControl = this.fieldForm.controls[this.language.iso2Code];
        } else {
            this.selectedFormControl = this.fieldForm.controls[fieldInvariant];
        }
    }

    public selectLanguage(language: AppLanguageDto) {
        this.selectedFormControl['_clearChangeFns']();
        this.selectedFormControl = this.fieldForm.controls[language.iso2Code];

        this.language = language;
    }
}

