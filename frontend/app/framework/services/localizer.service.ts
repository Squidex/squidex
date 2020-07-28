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

export enum PipeOptions {
    TRANSLATE = 'translate',
    LOWER =  'lower',
    UPPER = 'upper'
}

// TODO refactor this file, substract helper functions
@Injectable()
export class LocalizerService {
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

    public get(key: string, args?: ReadonlyArray<object>): any {
        let text: string;

        if (key.startsWith('i18n:')) {
            return this.get(key.substring(5), args);
        }

        if (!this.translations[key]) {
            return key;
        }

        text = this.translations[key];

        if (args && Object.keys(args).length > 0) {
           text = this.replaceVariables(text, args);
        }

        return text;
    }

    private replaceVariables(text: string, args: ReadonlyArray<object>): string {
        const regex = new RegExp(Object.keys(args).join('|'), 'g');

        return text.replace(regex, (matched: string) => {
            let replaceValue = args[matched];
            if (matched.includes('|')) {
                replaceValue = this.handlePipeOption(matched, args[matched]);
            }
            return replaceValue;
        });
    }

    private handlePipeOption(match: string, argument: string) {
        const foundPipeOption = match.split('|')[1];
        foundPipeOption.substring(0, foundPipeOption.length - 1);

        if (!foundPipeOption) {
            return argument;
        }

        switch (foundPipeOption[1]) {
            case PipeOptions.TRANSLATE: {
                return this.get(argument);
            }

            case PipeOptions.LOWER: {
                return argument.charAt(0).toLowerCase() + argument.slice(1);
            }

            case PipeOptions.UPPER: {
                return argument.charAt(0).toUpperCase() + argument.slice(1);
            }
            default: {
                console.log('default');
                return argument;
            }
        }
    }

}