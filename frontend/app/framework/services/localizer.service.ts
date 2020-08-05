/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export const LocalizerServiceServiceFactory = (translations: Object, logMissingKeys: boolean) => {
    return new LocalizerService(translations, logMissingKeys);
};

export enum PipeOptions {
    TRANSLATE = 'translate',
    LOWER = 'lower',
    UPPER = 'upper'
}

@Injectable()
export class LocalizerService {
    constructor(
        private readonly translations: Object,
        private readonly logMissingKeys: boolean
    ) {
    }

    public get(key: string, args?: ReadonlyArray<object>): any {
        let text: string;

        if (key.startsWith('i18n:')) {
            return this.get(key.substring(5), args);
        }

        if (!this.translations[key]) {
            if (this.logMissingKeys) {
                console.warn('Missing i18n key: {key}');
            }

            return key;
        }

        text = this.translations[key];

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