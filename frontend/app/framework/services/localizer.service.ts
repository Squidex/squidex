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

enum PipeOptions {
    TRANSLATE = 'translate',
    LOWER =  'lower',
    UPPER = 'upper'
}

// TODO refactor this file, substract helper functions
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

        if (key.startsWith('i18n:')) {
            return this.get(key.substring(5), args);
        }

        if (!this.translations[key]) {
            return key;
        }

        text = this.translations[key];

        if (args && args?.length > 0) {
           text = this.replaceVariables(text, args);
        }

        return text;
    }

    private replaceVariables(text: string, args: readonly any[]): string {
        const regex = new RegExp('\{(.*?)\}', 'g');
        let index = -1;
        return text.replace(regex, match => {
            index++;

            let replaceValue = args[index];
            if (match.includes('|')) {
                replaceValue = this.handlePipeOption(match, args[index]);
            }
            return replaceValue;
        });
    }

    private handlePipeOption(match: string, arg: string) {
        const regex = new RegExp('\\|(.*?)\}', 'g');

        const foundPipeOption = regex.exec(match);

        if (!foundPipeOption) {
            return arg;
        }

        switch (foundPipeOption[1]) {
            case PipeOptions.TRANSLATE: {
                return this.get(arg);
            }

            case PipeOptions.LOWER: {
                return arg.charAt(0).toLowerCase() + arg.slice(1);
            }

            case PipeOptions.UPPER: {
                return arg.charAt(0).toUpperCase() + arg.slice(1);
            }
            default: {
                console.log('default');
                return arg;
            }
        }
    }

}