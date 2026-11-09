/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Service } from '@angular/core';
import { TagConverter, TagValue } from '@app/framework';
import { SchemaDto } from '../model';
import { SchemasState } from './schemas.state';

class SchemaConverter implements TagConverter {
    public suggestions: ReadonlyArray<TagValue>;

    constructor(
        private readonly schemas: ReadonlyArray<SchemaDto>,
        normalOnly: boolean,
    ) {
        if (normalOnly) {
            schemas = schemas.filter(x => x.type === 'Default');
        }

        this.suggestions = schemas.map(x => new TagValue(x.id, x.name, x.id));
    }

    public convertInput(input: string) {
        const schema = this.schemas.find(x => x.name === input);

        if (schema) {
            return new TagValue(schema.id, schema.name, schema.id);
        }

        return null;
    }

    public convertValue(value: any) {
        const schema = this.schemas.find(x => x.id === value);

        if (schema) {
            return new TagValue(schema.id, schema.name, schema.id);
        }

        return null;
    }
}

@Service()
export class SchemaTagSource {
    public readonly schemasState = inject(SchemasState);

    public readonly converter = computed(() => {
        return new SchemaConverter(this.schemasState.schemas(), false);
    });

    public readonly normalConverter = computed(() => {
        return new SchemaConverter(this.schemasState.schemas(), true);
    });

    constructor() {
        this.schemasState.loadIfNotLoaded();
    }
}
