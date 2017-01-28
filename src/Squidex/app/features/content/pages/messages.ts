/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class ContentCreated {
    constructor(
        public readonly id: string,
        public readonly data: any
    ) {
    }
}

export class ContentUpdated {
    constructor(
        public readonly id: string,
        public readonly data: any
    ) {
    }
}

export class ContentDeleted {
    constructor(
        public readonly id: string
    ) {
    }
}