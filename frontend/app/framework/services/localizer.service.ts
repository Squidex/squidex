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

    public get(key: string, args?: readonly any[]): any {
        let text: string;
        text = key;

        if (args) {
           text = this.replaceVariables(text, args);
        }
        return text;

    }
    private replaceVariables(text: string, args: readonly any[]): string {
        const reggex = new RegExp('\{{(.*?)\}}', 'g');
        let i = -1;
        return text.replace(reggex, () => {
            i++;
            return args[i];
          });
    }

    public getTranslatedValue(text: string): string {
        if (this.doesStringStartWithi18n(text)) {
            return this.get(text.substring(5));
        }
        return text;
    }

    public doesStringStartWithi18n(text: string): boolean {
        return text.startsWith('i18n:');
    }
}