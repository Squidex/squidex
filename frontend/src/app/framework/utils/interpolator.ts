/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from './date-time';
import { Types } from './types';

const regex = /\$?{([^}]+)}/g;

export function interpolate(pattern: string, value?: any, shortcut?: string, fallback = 'undefined'): string {
    const result = pattern.replace(regex, (_: string, path: string) => {
        let replaced = value;

        for (const segment of path.split('.')) {
            const trimmed = segment.trim();

            if (!replaced) {
                return false;
            }

            if (Types.isArray(replaced)) {
                replaced = replaced[Number.parseInt(trimmed, 10)];
            } else {
                replaced = replaced[trimmed];
            }
        }

        if (Types.isString(replaced)) {
            return replaced;
        } else if (Types.isNumber(replaced)) {
            return replaced.toString();
        } else if (Types.is(replaced, DateTime)) {
            return replaced.toISOString();
        } else if (Types.isObject(replaced) && shortcut) {
            return replaced[shortcut] || fallback;
        }

        return fallback;
    });

    return result;
}
