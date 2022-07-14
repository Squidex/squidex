/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { TagConverter, TagValue } from '@app/framework';
import { SchemaDto } from './../services/schemas.service';
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

@Injectable()
export class SchemaTagSource {
    public converter =
        this.schemasState.schemas.pipe(
            map(x => new SchemaConverter(x, false)));

    public normalConverter =
        this.schemasState.schemas.pipe(
            map(x => new SchemaConverter(x, true)));

    constructor(
        readonly schemasState: SchemasState,
    ) {
        this.schemasState.loadIfNotLoaded();
    }
}
