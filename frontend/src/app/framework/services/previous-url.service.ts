/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { Router, RoutesRecognized } from '@angular/router';
import { filter, pairwise } from 'rxjs/operators';

@Service()
export class PreviousUrl {
    private readonly router = inject(Router);
    private previousPath = '';

    constructor() {
        this.previousPath = this.router.url;

        this.router.events
            .pipe(filter(e => e instanceof RoutesRecognized), pairwise())
            .subscribe((event: any[]) => {
                this.previousPath = event[0].urlAfterRedirects;
            });
    }

    public isPath(path: string) {
        const trim = (value: string) => {
            if (!value) {
                return value;
            }

            if (value.indexOf('?') > 0) {
                value = value.split('?')[0];
            }

            value = value.replace(TRIM_REGEX, '');
            return value;
        };

        return trim(this.previousPath) === trim(path);
    }
}

const TRIM_REGEX = /^[\s\/]+|[\s\/]+$/g;