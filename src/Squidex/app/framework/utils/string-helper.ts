/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module StringHelper {
    export function firstNonEmpty(...values: (string | undefined | null)[]) {
        for (let value of values) {
            if (value) {
                value = value.trim();

                if (value.length > 0) {
                    return value;
                }
            }
        }

        return '';
    }

    export function toCamelCase(value: string) {
        return value.replace(/[^\s\-_]+/g, (w, i) => (i === 0 ? w[0].toLowerCase() : w[0].toUpperCase()) + w.slice(1)).replace(/[\s\-_]+/g, '').trim();
    }
}