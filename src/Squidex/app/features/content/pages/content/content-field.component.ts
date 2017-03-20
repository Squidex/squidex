/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { AppLanguageDto, FieldDto } from 'shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
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

    public fieldLanguages: string[];
    public fieldLanguage: string;

    public selectLanguage(language: AppLanguageDto) {
        this.fieldLanguage = language.iso2Code;
    }

    public ngOnInit() {
        if (this.field.isDisabled) {
            this.fieldForm.disable();
        }

        if (this.field.properties.isLocalizable) {
            this.fieldLanguages = this.languages.map(t => t.iso2Code);
            this.fieldLanguage = this.fieldLanguages[0];
        } else {
            this.fieldLanguages = ['iv'];
            this.fieldLanguage = 'iv';
        }
    }
}

