/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export function nextBy<T>(updater: (value: T) => T): void {
    return this.next(updater(this.value));
}