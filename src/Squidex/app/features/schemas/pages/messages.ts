/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { SchemaDto } from 'shared';

export class SchemaUpdated {
    constructor(
        public readonly schema: SchemaDto
    ) {
    }
}

export class SchemaDeleted {
    constructor(
        public readonly schema: SchemaDto
    ) {
    }
}