/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SchemaDto } from 'shared';

export class SchemaUpdated {
    constructor(
        public readonly schema: SchemaDto
    ) {
    }
}

export class SchemaCreated {
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

export class SchemaCloning {
    constructor(
        public readonly importing: any
    ) {
    }
}