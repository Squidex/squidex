/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Router, RoutesRecognized } from '@angular/router';
import { filter, pairwise } from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})
export class PreviousUrl {
    private previousPath = '';

    constructor(
        private readonly router: Router,
    ) {
        this.previousPath = router.url;

        this.router.events
            .pipe(filter(e => e instanceof RoutesRecognized), pairwise())
            .subscribe((event: any[]) => {
                this.previousPath = event[0].urlAfterRedirects;
            });
    }

    public pathStartsWith(path: string) {
        return this.previousPath && this.previousPath.startsWith(path);
    }
}