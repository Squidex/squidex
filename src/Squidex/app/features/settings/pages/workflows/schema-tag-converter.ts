/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Converter, SchemaDto, TagValue } from '@app/shared';

export class SchemaTagConverter implements Converter {
    public readonly suggestions: TagValue[];

    constructor(
        private readonly schemas: SchemaDto[]
    ) {
        this.suggestions = schemas.map(x => new TagValue(x.id, x.name, x.id));
    }

    public convertInput(input: string): TagValue<any> | null {
        const schema = this.schemas.find(x => x.name === input);

        if (schema) {
            return new TagValue(schema.id, schema.name, schema.id);
        }

        return null;
    }

    public convertValue(value: any): TagValue<any> | null {
        const schema = this.schemas.find(x => x.id === value);

        if (schema) {
            return new TagValue(schema.id, schema.name, schema.id);
        }

        return null;
    }
}