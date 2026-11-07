/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class ClipboardService {
    private readonly textValue = signal('');

    public readonly text = this.textValue.asReadonly();

    public setText(text: any) {
        this.textValue.set(text || '');
    }
}
