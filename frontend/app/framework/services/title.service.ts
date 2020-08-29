/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Injectable } from '@angular/core';
import { LocalizerService } from './localizer.service';

export class TitlesConfig {
    constructor(
        public readonly prefix?: string,
        public readonly suffix?: string,
        public readonly separator?: string
    ) {
    }
}

export const TitleServiceFactory = (titles: TitlesConfig, localizer: LocalizerService) => {
    return new TitleService(titles, localizer);
};

@Injectable()
export class TitleService {
    private readonly stack: any[] = [];

    constructor(
        private readonly titles: TitlesConfig,
        private readonly localizer: LocalizerService
    ) {
        this.titles = new TitlesConfig(
            this.localizer.getOrKey(titles.prefix),
            this.localizer.getOrKey(titles.suffix),
            this.titles.separator
        );

        this.updateTitle();
    }

    public push(value: any, previous?: any) {
        if (value) {
            const lastIndex = this.stack.length - 1;

            if (previous && this.stack[lastIndex] === previous) {
                this.stack[lastIndex] = value;
            } else {
                this.stack.push(value);
            }

            this.updateTitle();
        }
    }

    public pop() {
        this.stack.pop();

        this.updateTitle();
    }

    private updateTitle() {
        const { prefix, separator, suffix } = this.titles;

        let title = '';

        if (this.stack.length > 0) {
            title = this.stack.map(x => this.localizer.getOrKey(x)).join(separator || ' | ');
        }

        if (title) {
            if (prefix) {
                title = `${prefix} - ${title}`;
            }

            if (suffix) {
                title = `${title} - ${suffix}`;
            }
        } else if (suffix) {
            title = suffix;
        } else if (prefix) {
            title = prefix;
        }

        document.title = title;
    }
}