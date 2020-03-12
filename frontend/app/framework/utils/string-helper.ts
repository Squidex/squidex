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

    export function appendToUrl(url: string, key: string, value: any) {
        if (url.indexOf('?') > 0) {
            url += '&';
        } else {
            url += '?';
        }

        url += `${key}=${value}`;

        return url;
    }
}