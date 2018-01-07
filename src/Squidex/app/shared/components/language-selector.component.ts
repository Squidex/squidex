/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';

import { fadeAnimation, ModalView } from 'framework';

export interface Language { iso2Code: string; englishName: string; isMasterLanguage: true; }

@Component({
    selector: 'sqx-language-selector',
    styleUrls: ['./language-selector.component.scss'],
    templateUrl: './language-selector.component.html',
    animations: [
        fadeAnimation
    ]
})
export class LanguageSelectorComponent implements OnChanges, OnInit {
    public dropdown = new ModalView(false, true);

    @Input()
    public size: string;

    @Input()
    public languages: Language[] = [];

    @Input()
    public selectedLanguage: Language;

    @Output()
    public selectedLanguageChanged = new EventEmitter<Language>();

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
        this.selectedLanguageChanged.emit(language);
    }
}