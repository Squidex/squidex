/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ContentDto } from 'shared';

export class ContentCreated {
    constructor(
        public readonly content: ContentDto
    ) {
    }
}

export class ContentUpdated {
    constructor(
        public readonly content: ContentDto
    ) {
    }
}

export class ContentRemoved {
    constructor(
        public readonly content: ContentDto
    ) {
    }
}

export class ContentVersionSelected {
    constructor(
        public readonly version: number
    ) {
    }
}