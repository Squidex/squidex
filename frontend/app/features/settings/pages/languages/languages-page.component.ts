/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { AppLanguageDto, LanguagesState } from '@app/shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent implements OnInit {
    constructor(
        public readonly languagesState: LanguagesState
    ) {
    }

    public ngOnInit() {
        this.languagesState.load();
    }

    public reload() {
        this.languagesState.load(true);
    }

    public trackByLanguage(_index: number, language: { language: AppLanguageDto }) {
        return language.language.iso2Code;
    }
}