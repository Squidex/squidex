/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { fadeAnimation, ModalModel } from '@app/framework/internal';

export interface Language { iso2Code: string; englishName: string; isMasterLanguage?: boolean }

@Component({
    selector: 'sqx-language-selector[language][languages]',
    styleUrls: ['./language-selector.component.scss'],
    templateUrl: './language-selector.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageSelectorComponent implements OnChanges, OnInit {
    @Output()
    public languageChange = new EventEmitter<any>();

    @Input()
    public language: Language;

    @Input()
    public languages: ReadonlyArray<Language> = [];

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'md';

    public dropdown = new ModalModel();

    public get isSmallMode(): boolean {
        return this.languages && this.languages.length > 0 && this.languages.length <= 3;
    }

    public get isLargeMode(): boolean {
        return this.languages && this.languages.length > 3;
    }

    public ngOnChanges() {
        this.update();
    }

    public ngOnInit() {
        this.update();
    }

    private update() {
        if (this.languages && this.languages.length > 0 && (!this.language || this.languages.indexOf(this.language) < 0)) {
            const selectedLanguage =
                this.languages.find(l => l.isMasterLanguage) ||
                this.languages[0];

            this.selectLanguage(selectedLanguage);
        }
    }

    public selectLanguage(language: Language) {
        if (language?.iso2Code !== this.language?.iso2Code) {
            this.language = language;
            this.languageChange.emit(language);
        }
    }

    public trackByLanguage(_index: number, language: Language) {
        return language.iso2Code;
    }
}
