/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export function loadWithDefaults<T extends object>(value: object, defaults: T): T {
    const source = value as Record<string, unknown>;
    const result = { ...defaults } as Record<string, unknown>;

    for (const key of Object.keys(defaults)) {
        const loaded = source[key];
        if (loaded !== null && loaded !== undefined) {
            result[key] = loaded;
        }
    }

    return result as T;
}

export function submitWithoutEmpty<T extends object>(value: T): Record<string, unknown> {
    const result = { ...value } as Record<string, unknown>;

    for (const [key, submitted] of Object.entries(result)) {
        if (submitted === '' || Number.isNaN(submitted)) {
            result[key] = undefined;
        }
    }

    return result;
}
