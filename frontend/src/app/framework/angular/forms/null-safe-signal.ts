/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { signal, untracked, WritableSignal } from '@angular/core';
import { Types } from '@app/framework/internal';

export function nullSafeSignal<T>(initial: T): WritableSignal<T> {
    // Signal forms remove fields with undefined values, therefore we store null instead.
    const result = signal(replaceUndefined(initial));
    const set = result.set.bind(result);

    result.set = value => set(replaceUndefined(value));
    result.update = fn => result.set(fn(untracked(result)));

    return result;
}

function replaceUndefined<T>(value: T): T {
    if (Types.isArray(value)) {
        let copy: any[] | undefined;

        for (let i = 0; i < value.length; i++) {
            const source = value[i];
            const target = Types.isUndefined(source) ? null : replaceUndefined(source);

            if (target !== source) {
                copy ??= [...value];
                copy[i] = target;
            }
        }

        return (copy ?? value) as T;
    }

    if (Types.isObject(value)) {
        let copy: Record<string, any> | undefined;

        for (const [key, source] of Object.entries(value)) {
            const target = Types.isUndefined(source) ? null : replaceUndefined(source);

            if (target !== source) {
                copy ??= { ...value };
                copy[key] = target;
            }
        }

        return (copy ?? value) as T;
    }

    return value;
}
