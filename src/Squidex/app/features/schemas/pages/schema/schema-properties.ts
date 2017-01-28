/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class SchemaPropertiesDto {
    constructor(
        public readonly name: string,
        public readonly label: string,
        public readonly hints: string
    ) {
    }
}