/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export module StringHelper {
    export function firstNonEmpty(...values: string[]) {
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
}