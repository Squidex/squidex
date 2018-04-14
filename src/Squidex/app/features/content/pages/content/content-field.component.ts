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
    fieldInvariant
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
    public languages: AppLanguageDto[];

    @Input()
    public contentFormSubmitted: boolean;

    public selectedFormControl: AbstractControl;
    public selectedLanguage: AppLanguageDto;

    public ngOnInit() {
        const masterLanguage = this.languages[0];

        if (this.field.isLocalizable) {
            this.selectedFormControl = this.fieldForm.controls[masterLanguage.iso2Code];
        } else {
            this.selectedFormControl = this.fieldForm.controls[fieldInvariant];
        }

        this.selectedLanguage = masterLanguage;
    }

    public selectLanguage(language: AppLanguageDto) {
        this.selectedFormControl['_clearChangeFns']();

        this.selectedFormControl = this.fieldForm.controls[language.iso2Code];
        this.selectedLanguage = language;
    }
}

