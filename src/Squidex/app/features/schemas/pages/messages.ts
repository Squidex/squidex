/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { SchemaPropertiesDto } from 'shared';

export class SchemaUpdated {
    constructor(
        public readonly name: string,
        public readonly properties: SchemaPropertiesDto,
        public readonly isPublished: boolean,
        public readonly version: string
    ) {
    }
}

export class SchemaDeleted {
    constructor(
        public readonly name: string
    ) {
    }
}