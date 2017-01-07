/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class SchemaUpdated {
    constructor(
        public readonly name: string,
        public readonly isPublished: boolean
    ) {
    }
}