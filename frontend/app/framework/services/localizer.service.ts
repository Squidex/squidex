/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export const LocalizerServiceFactory = (translations: Object) => {
    return new LocalizerService(translations);
};

export enum PipeOptions {
    TRANSLATE = 'translate',
    LOWER = 'lower',
    UPPER = 'upper'
}

@Injectable()
export class LocalizerService {
    private shouldLog = false;

    constructor(
        private readonly translations: Object
    ) {
    }

    public logMissingKeys() {
        this.shouldLog = true;

        return this;
    }

    public get(key: string | undefined, args?: any): string {
        if (!key) {
            return key || '';
        }

        if (key.startsWith('i18n:')) {
            return this.get(key.substring(5), args);
        }

        let text = this.translations[key];

        if (!text) {
            if (this.shouldLog) {
                console.warn('Missing i18n key: {key}');
            }

            return key;
        }

        if (args && Object.keys(args).length > 0) {
            text = this.replaceVariables(text, args);
        }

        return text;
    }

    private replaceVariables(text: string, args: ReadonlyArray<object>): string {
        while (true) {
            const indexOfStart = text.indexOf('{');

            if (indexOfStart < 0) {
                break;
            }

            const indexOfEnd = text.indexOf('}');

            const replace = text.substring(indexOfStart, indexOfEnd + 1);

            text = text.replace(replace, (matched: string) => {
                let replaceValue: string;

                if (matched.includes('|')) {
                    const splittedValue = matched.split('|');

                    replaceValue = this.handlePipeOption(args[splittedValue[0].substr(1)], splittedValue[1].slice(0, -1));
                } else {
                    const key = matched.substring(1, matched.length - 1);

                    replaceValue = args[key];
                }

                return replaceValue;
            });
        }

        return text;
    }

    private handlePipeOption(value: string, pipeOption: string) {
        if (!pipeOption) {
            return value;
        }

        switch (pipeOption) {
            case PipeOptions.TRANSLATE: {
                return this.get(value);
            }

            case PipeOptions.LOWER: {
                return value.charAt(0).toLowerCase() + value.slice(1);
            }

            case PipeOptions.UPPER: {
                return value.charAt(0).toUpperCase() + value.slice(1);
            }
            default: {
                return value;
            }
        }
    }

}