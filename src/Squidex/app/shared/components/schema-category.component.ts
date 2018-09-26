/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    fadeAnimation,
    ImmutableArray,
    LocalStoreService,
    SchemaDetailsDto,
    SchemaDto,
    SchemasState,
    Types
} from '@app/shared/internal';

@Component({
    selector: 'sqx-schema-category',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaCategoryComponent implements OnInit, OnChanges {
    @Output()
    public removing = new EventEmitter();

    @Input()
    public name: string;

    @Input()
    public isReadonly: boolean;

    @Input()
    public schemasFilter: string;

    @Input()
    public schemas: ImmutableArray<SchemaDto>;

    public displayName: string;

    public schemasFiltered: ImmutableArray<SchemaDto>;
    public schemasForCategory: ImmutableArray<SchemaDto>;

    public isOpen = true;

    public allowDrop = (schema: any) => {
        return (Types.is(schema, SchemaDto) || Types.is(schema, SchemaDetailsDto)) && !this.isSameCategory(schema);
    }

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.isOpen = this.localStore.get(`schema-category.${name}`) !== 'false';
    }

    public toggle() {
        this.isOpen = !this.isOpen;

        this.localStore.set(`schema-category.${name}`, this.isOpen + '');
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes['schemas'] || changes['schemasFilter']) {
            const query = this.schemasFilter;

            this.schemasForCategory = this.schemas.filter(x => this.isSameCategory(x));
            this.schemasFiltered = this.schemasForCategory.filter(x => !query || x.name.indexOf(query) >= 0);
        }

        if (changes['name']) {
            if (!this.name || this.name.length === 0) {
                this.displayName = 'Schemas';
            } else {
                this.displayName = this.name;
            }
        }
    }

    private isSameCategory(schema: SchemaDto): boolean {
        return (!this.name && !schema.category) || schema.category === this.name;
    }

    public changeCategory(schema: SchemaDto) {
        this.schemasState.changeCategory(schema, this.name).pipe(onErrorResumeNext()).subscribe();
    }

    public trackBySchema(index: number, schema: SchemaDto) {
        return schema.id;
    }
}