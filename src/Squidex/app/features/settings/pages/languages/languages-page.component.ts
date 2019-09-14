/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppLanguageDto,
    AppsState,
    LanguagesState
} from '@app/shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent implements OnInit {
    constructor(
        public readonly appsState: AppsState,
        public readonly languagesState: LanguagesState
    ) {
    }

    public ngOnInit() {
        this.languagesState.load();
    }

    public reload() {
        this.languagesState.load(true);
    }

    public trackByLanguage(index: number, language: { language: AppLanguageDto }) {
        return language.language.iso2Code;
    }
}