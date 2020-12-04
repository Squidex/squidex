/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { FieldDto, LanguageDto } from '@app/shared';
import { Language } from '../../../../../../framework/angular/language-selector.component';
import { Observable } from 'rxjs';

class LanguageCommon implements Language {
    
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public readonly isMasterLanguage: true) {}    
} 


const ALL_LANGUAGES: ReadonlyArray<LanguageDto> = [
    new LanguageDto('en', 'English'),
    new LanguageDto ('nl', 'Nederlands'), 
    new LanguageDto ('it', 'Italiano')];

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html'
})
export class FieldFormCommonComponent implements OnInit {
    public readonly standalone = { standalone: true };

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto> = ALL_LANGUAGES;

    public showDefaultValue: Observable<string>;

    public ngOnInit() {
        this.fieldForm.setControl('localizedLabel',
            new FormControl(this.field.properties.localizedLabel));

        this.fieldForm.setControl('localizedHints',
            new FormControl(this.field.properties.localizedHints));   
    }
}