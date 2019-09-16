/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Subscription } from 'rxjs';

import {
    Converter,
    SchemaDto,
    SchemasState,
    TagValue
} from '@app/shared';

export class SchemaTagConverter implements Converter {
    private schemasSubscription: Subscription;
    private schemas: SchemaDto[] = [];

    public suggestions: TagValue[] = [];

    constructor(
        readonly schemasState: SchemasState
    ) {
        this.schemasSubscription =
            schemasState.changes.subscribe(state => {
                if (state.isLoaded) {
                    this.schemas = state.schemas.values;

                    this.suggestions = this.schemas.map(x => new TagValue(x.id, x.name, x.id));
                }
            });
    }

    public destroy() {
        this.schemasSubscription.unsubscribe();
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