/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FloatingPlacement, ModalModel } from '@app/framework/internal';

export interface Language { iso2Code: string; englishName: string; isMasterLanguage?: boolean }

@Component({
    selector: 'sqx-language-selector',
    styleUrls: ['./language-selector.component.scss'],
    templateUrl: './language-selector.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageSelectorComponent implements  OnInit {
    @Output()
    public languageChange = new EventEmitter<any>();

    @Input({ required: true })
    public language!: Language;

    @Input({ required: true })
    public languages: ReadonlyArray<Language> = [];

    @Input()
    public exists?: { [language: string]: boolean } | null;

    @Input()
    public percents?: { [language: string]: number } | null;

    @Input()
    public dropdownPosition: FloatingPlacement = 'bottom-end';

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'md';

    public dropdown = new ModalModel();

    public ngOnChanges() {
        this.update();
    }

    public ngOnInit() {
        this.update();
    }

    private update() {
        if (this.languages?.length > 0 && (!this.language || !this.languages.includes(this.language))) {
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
