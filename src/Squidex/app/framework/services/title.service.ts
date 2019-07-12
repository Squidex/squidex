/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export class TitlesConfig {
    constructor(
        public readonly value: { [key: string]: string },
        public readonly prefix?: string,
        public readonly suffix?: string
    ) {
    }
}

export const TitleServiceFactory = (titles: TitlesConfig) => {
    return new TitleService(titles);
};

@Injectable()
export class TitleService {
    constructor(private readonly titles: TitlesConfig) {}

    public setTitle(key: string, parameters?: { [key: string]: string }) {
        let title = this.titles.value[key] || key;

        if (!title) {
            return;
        }

        if (parameters) {
            for (let parameter in parameters) {
                if (parameters.hasOwnProperty(parameter)) {
                    title = title.replace(`{${parameter}}`, parameters[parameter]);
                }
            }
        }

        if (this.titles.prefix) {
            title = this.titles.prefix + ' - ' + title;
        }

        if (this.titles.suffix) {
            title = title + ' - ' + this.titles.suffix;
        }

        document.title = title;
    }
}