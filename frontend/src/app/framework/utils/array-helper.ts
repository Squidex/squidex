/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export function compareStrings(a: string, b: string) {
    return a.localeCompare(b, undefined, { sensitivity: 'base' });
}
