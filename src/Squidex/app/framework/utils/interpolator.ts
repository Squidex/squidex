/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from './date-time';
import { Types } from './types';

const regex = /\${[^}]+}/;

export function interpolate(pattern: string, value?: any, shortcut?: string, fallback = 'undefined'): string {
    const result = pattern.replace(regex, (match: string) => {
        let replaced = value;

        const path = match.substr(2, match.length - 3).split('.');

        for (let segment of path) {
            if (Types.isObject(replaced)) {
                replaced = replaced[segment];
            } else if (Types.isArray(replaced)) {
                replaced = replaced[Number.parseInt(segment, 10)];
            } else {
                break;
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