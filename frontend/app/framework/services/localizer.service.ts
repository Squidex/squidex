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

        if (this.doesStringStartWithi18n(key)) {
            return this.get(key.substring(5), args);
        }

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

    public doesStringStartWithi18n(key: string): boolean {
        return key.startsWith('i18n:');
    }
}