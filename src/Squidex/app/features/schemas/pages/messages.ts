/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class SchemaUpdated {
    constructor(
        public readonly name: string,
        public readonly label: string,
        public readonly isPublished: boolean,
        public readonly version: string
    ) {
    }
}