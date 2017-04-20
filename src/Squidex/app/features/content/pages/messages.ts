/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class ContentCreated {
    constructor(
        public readonly id: string,
        public readonly data: any,
        public readonly version: string,
        public readonly isPublished: boolean
    ) {
    }
}

export class ContentUpdated {
    constructor(
        public readonly id: string,
        public readonly data: any,
        public readonly version: string
    ) {
    }
}

export class ContentPublished {
    constructor(
        public readonly id: string
    ) {
    }
}

export class ContentUnpublished {
    constructor(
        public readonly id: string
    ) {
    }
}

export class ContentDeleted {
    constructor(
        public readonly id: string
    ) {
    }
}