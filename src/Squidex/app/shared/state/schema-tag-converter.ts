/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { Converter, TagValue } from '@app/framework';

import { SchemaDto } from './../services/schemas.service';
import { SchemasState } from './schemas.state';

@Injectable()
export class SchemaTagConverter implements Converter, OnDestroy {
    private schemasSubscription: Subscription;
    private schemas: SchemaDto[] = [];

    public suggestions: TagValue[] = [];

    constructor(
        readonly schemasState: SchemasState
    ) {
        this.schemasSubscription =
            schemasState.schemas.subscribe(schemas => {
                this.schemas = schemas.values;

                this.suggestions = this.schemas.map(x => new TagValue(x.id, x.name, x.id));
            });

        this.schemasState.loadIfNotLoaded();
    }

    public ngOnDestroy() {
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