/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
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

@Injectable()
export class TitleService {
    private readonly path$ = new BehaviorSubject<ReadonlyArray<Title>>([]);

    public get pathChanges(): Observable<ReadonlyArray<Title>> {
        return this.path$;
    }

    constructor(
        private readonly titles: TitlesConfig,
        private readonly localizer: LocalizerService,
    ) {
        this.titles = new TitlesConfig(
            this.localizer.getOrKey(titles.prefix),
            this.localizer.getOrKey(titles.suffix),
            this.titles.separator,
        );

        this.path$.subscribe(value => {
            this.updateTitle(value);
        });
    }

    public push(value: string, previous?: string, route?: any) {
        if (value) {
            const clone = [...this.path$.value];

            const lastIndex = clone.length - 1;
            const localized = this.localizer.getOrKey(value);

            const title = { localized, value, route };

            if (previous && clone[lastIndex].value === previous) {
                clone[lastIndex] = title;
            } else {
                clone.push(title);
            }

            this.path$.next(clone);
        }
    }

    public pop() {
        const clone = [...this.path$.value];

        clone.pop();

        this.path$.next(clone);
    }

    private updateTitle(path: ReadonlyArray<Title>) {
        const { prefix, separator, suffix } = this.titles;

        let title = '';

        const cleaned = path.map(x => x.localized).filter(x => !!x);

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
