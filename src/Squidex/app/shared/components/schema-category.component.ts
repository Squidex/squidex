/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    fadeAnimation,
    ImmutableArray,
    LocalStoreService,
    SchemaDetailsDto,
    SchemaDto,
    SchemasState,
    StatefulComponent,
    Types
} from '@app/shared/internal';

interface State {
    displayName?: string;

    schemasFiltered: ImmutableArray<SchemaDto>;
    schemasForCategory: ImmutableArray<SchemaDto>;

    isOpen: boolean;
}

@Component({
    selector: 'sqx-schema-category',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SchemaCategoryComponent extends StatefulComponent<State> implements OnInit, OnChanges {
    @Output()
    public removing = new EventEmitter();

    @Input()
    public name: string;

    @Input()
    public isReadonly: boolean;

    @Input()
    public routeSingletonToContent = false;

    @Input()
    public schemasFilter: string;

    @Input()
    public schemas: ImmutableArray<SchemaDto>;

    public allowDrop = (schema: any) => {
        return (Types.is(schema, SchemaDto) || Types.is(schema, SchemaDetailsDto)) && !this.isSameCategory(schema);
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState
    ) {
        super(changeDetector, {
            schemasFiltered: ImmutableArray.empty(),
            schemasForCategory: ImmutableArray.empty(),
            isOpen: true
        });
    }

    public ngOnInit() {
        this.next(s => ({ ...s, isOpen: !this.localStore.getBoolean(this.configKey()) }));
    }

    public toggle() {
        this.next(s => ({ ...s, isOpen: !s.isOpen }));

        this.localStore.setBoolean(this.configKey(), !this.snapshot.isOpen);
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes['schemas'] || changes['schemasFilter']) {
            const isSameCategory = (schema: SchemaDto) => {
                return (!this.name && !schema.category) || schema.category === this.name;
            };

            const query = this.schemasFilter;

            const schemasForCategory = this.schemas.filter(x => isSameCategory(x));
            const schemasFiltered = schemasForCategory.filter(x => !query || x.name.indexOf(query) >= 0);

            let isOpen = false;

            if (query) {
                isOpen = true;
            } else {
                isOpen = this.localStore.get(`schema-category.${this.name}`) !== 'false';
            }

            this.next(s => ({ ...s, isOpen, schemasFiltered, schemasForCategory }));
        }

        if (changes['name']) {
            let displayName = 'Schemas';

            if (this.name && this.name.length > 0) {
                displayName = this.name;
            }

            this.next(s => ({ ...s, displayName }));
        }
    }

    public schemaRoute(schema: SchemaDto) {
        if (schema.isSingleton && this.routeSingletonToContent) {
            return [schema.name, schema.id];
        } else {
            return [schema.name];
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

    public schemaPermission(schema: SchemaDto) {
        return `?squidex.apps.{app}.schemas.${schema.name}.*;squidex.apps.{app}.contents.${schema.name}.*`;
    }

    private configKey(): string {
        return `squidex.schema.category.${this.name}.closed`;
    }
}
