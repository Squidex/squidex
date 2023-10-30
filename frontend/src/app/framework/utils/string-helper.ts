/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module StringHelper {
    export function firstNonEmpty(...values: ReadonlyArray<(string | undefined | null)>) {
        for (const value of values) {
            if (value) {
                const trimmed = value.trim();

                if (trimmed.length > 0) {
                    return trimmed;
                }
            }
        }

        return '';
    }

    export function appendToUrl(url: string, key: string, value?: any, ambersand = false) {
        if (url.includes('?') || ambersand) {
            url += '&';
        } else {
            url += '?';
        }

        if (value !== undefined) {
            url += `${key}=${value}`;
        } else {
            url += key;
        }

        return url;
    }

    export function appendLast(row: string, char: string) {
        const last = row[row.length - 1];

        if (last !== char) {
            return row + char;
        } else {
            return row;
        }
    }

    export function hashCode(value: string) {
        let hash = 0;

        if (!value || value.length === 0) {
            return hash;
        }

        for (let i = 0; i < value.length; i++) {
            const char = value.charCodeAt(i);

            hash = ((hash << 5) - hash) + char;
            hash |= 0;
        }

        return hash;
    }
}
