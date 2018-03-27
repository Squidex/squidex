/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class Version {
    constructor(
        public readonly value: string
    ) {
    }
}

export class Versioned<T> {
    constructor(
        public readonly version: Version,
        public readonly payload: T
    ) {
    }
}