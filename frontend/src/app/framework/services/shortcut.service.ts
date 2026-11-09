/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Service } from '@angular/core';
import * as Mousetrap from 'mousetrap';

@Service()
export class ShortcutService {
    public listen(keys: string, callback: (e: KeyboardEvent, combo: string) => void): () => void {
        const trimmed = keys.toLowerCase().replace(/\s/g, '').split(',');

        Mousetrap.bind(trimmed, (event: any, combo: any) => {
            return callback(event, combo);
        });

        return () => {
            Mousetrap.unbind(trimmed);
        };
    }

    public raise(keys: string) {
        Mousetrap.trigger(keys);
    }
}
