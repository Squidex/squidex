/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module StringHelper {
    export function firstNonEmpty(...values: ReadonlyArray<(string | undefined | null)>) {
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

    export function appendToUrl(url: string, key: string, value?: any, ambersand = false) {
        if (url.indexOf('?') >= 0 || ambersand) {
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
}