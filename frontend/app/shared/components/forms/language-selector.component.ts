/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { fadeAnimation, LanguageDto, ModalModel } from '@app/shared/internal';

export interface Language { iso2Code: string; englishName: string; isMasterLanguage: true; }

@Component({
    selector: 'sqx-language-selector',
    styleUrls: ['./language-selector.component.scss'],
    templateUrl: './language-selector.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LanguageSelectorComponent implements OnChanges, OnInit {
    @Output()
    public selectedLanguageChange = new EventEmitter<Language>();

    @Input()
    public size: string;

    @Input()
    public languages: ReadonlyArray<Language> = [];

    @Input()
    public selectedLanguage: Language;

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
        if (this.languages && this.languages.length > 0 && (!this.selectedLanguage || this.languages.indexOf(this.selectedLanguage) < 0)) {
            const selectedLanguage =
                this.languages.find(l => l.isMasterLanguage) ||
                this.languages[0];

            this.selectLanguage(selectedLanguage);
        }
    }

    public selectLanguage(language: Language) {
        this.selectedLanguage = language;
        this.selectedLanguageChange.emit(language);
    }

    public trackByLanguage(_index: number, language: LanguageDto) {
        return language.iso2Code;
    }
}