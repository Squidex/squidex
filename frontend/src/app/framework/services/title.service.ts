/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service, signal, untracked } from '@angular/core';
import { Types } from '../utils/types';
import { LocalizerService } from './localizer.service';

export class TitlesConfig {
    constructor(
        public readonly prefix?: string,
        public readonly suffix?: string,
        public readonly separator?: string,
    ) {
    }
}

export type Title = { route?: any; localized: string; value: string };

@Service()
export class TitleService {
    private readonly localizer = inject(LocalizerService);
    private readonly titles = inject(TitlesConfig);
    private readonly pathValue = signal<ReadonlyArray<Title>>([]);

    public readonly path = this.pathValue.asReadonly();

    constructor() {
        this.titles = new TitlesConfig(
            this.localizer.getOrKey(this.titles.prefix),
            this.localizer.getOrKey(this.titles.suffix),
            this.titles.separator,
        );

        this.updateTitle([]);
    }

    public push(value: string, index?: number, route?: any) {
        let result: number | undefined;
        if (!value) {
            return result;
        }

        const clone = [...untracked(this.pathValue)];

        const lastIndex = clone.length - 1;
        const localized = this.localizer.getOrKey(value);

        const title = { localized, value, route };

        if (Types.isNumber(index) && index >= 0 && index <= lastIndex) {
            clone[index] = title;

            result = index;
        } else {
            clone.push(title);

            result = lastIndex + 1;
        }

        this.setPath(clone);
        return result;
    }

    public pop() {
        const clone = [...untracked(this.pathValue)];

        clone.pop();

        this.setPath(clone);
    }

    private setPath(path: ReadonlyArray<Title>) {
        this.pathValue.set(path);

        this.updateTitle(path);
    }

    private updateTitle(path: ReadonlyArray<Title>) {
        const { prefix, separator, suffix } = this.titles;

        let title = '';

        const cleaned = path.map(x => x.localized).defined();

        if (cleaned.length > 0) {
            title = cleaned.join(separator || ' | ');
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
