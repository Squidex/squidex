/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class PreviousUrl {
    public pathStartsWith(path: string) {
        try {
            const url = new URL(document.referrer);

            return url.pathname.startsWith(path);
        } catch {
            return false;
        }
    }
}