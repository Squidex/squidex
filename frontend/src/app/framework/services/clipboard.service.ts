/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Service, signal } from '@angular/core';

@Service()
export class ClipboardService {
    private readonly textValue = signal('');

    public readonly text = this.textValue.asReadonly();

    public setText(text: any) {
        this.textValue.set(text || '');
    }
}
