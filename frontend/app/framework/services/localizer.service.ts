/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export const LocalizerServiceServiceFactory = () => {
    return new LocalizerService();
};

@Injectable()
export class LocalizerService {
    private static instance: LocalizerService;
    public translations: Object;

    constructor() {
        if (process.env.NODE_ENV !== 'production') {
            console.log('dev mode, reading from json');
            // TODO make it possible to change the language.
            this.translations = require('./../../i18n/texts.en.json');
        } else {
            console.log('prod mode, reading from window option');
        }
    }

    public static getInstance(): LocalizerService {
        if (!LocalizerService.instance) {
            LocalizerService.instance = new LocalizerService();
        }

        return LocalizerService.instance;
    }

    public get(key: string, args?: readonly any[]): any {
        let text: string;

        if (this.doesStringStartWithi18n(key)) {
            return this.get(key.substring(5), args);
        }

        text = this.translations[key];

        if (args && args?.length > 0) {
           text = this.replaceVariables(text, args);
        }

        return text;

    }
    private replaceVariables(text: string, args: readonly any[]): string {
        const reggex = new RegExp('\{{(.*?)\}}', 'g');
        let index = -1;
        return text.replace(reggex, () => {
            index++;
            return args[index];
          });
    }

    public doesStringStartWithi18n(key: string): boolean {
        return key.startsWith('i18n:');
    }
}