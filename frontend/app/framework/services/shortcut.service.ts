/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import * as Mousetrap from 'mousetrap';

@Injectable()
export class ShortcutService {
    public on(keys: string, callback: (e: KeyboardEvent, combo: string) => void) {
        return Mousetrap.bind(keys, (event: any, combo: any) => {
            return callback(event, combo);
        });
    }

    public off(keys: string) {
        Mousetrap.unbind(keys);
    }

    public trigger(keys: string) {
        Mousetrap.trigger(keys);
    }
}
