/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';

import {
    fadeAnimation,
    ImmutableArray,
    isSameCategory,
    LocalStoreService,
    SchemaCategory,
    SchemaDetailsDto,
    SchemaDto,
    SchemasState,
    StatefulComponent,
    Types
} from '@app/shared/internal';

interface State {
    filtered: ImmutableArray<SchemaDto>;

    isOpen?: boolean;
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
    @Input()
    public schemaCategory: SchemaCategory;

    @Input()
    public schemasFilter: string;

    @Input()
    public forContent: boolean;

    @Output()
    public remove = new EventEmitter();

    public allowDrop = (schema: any) => {
        return (Types.is(schema, SchemaDto) || Types.is(schema, SchemaDetailsDto)) && !isSameCategory(this.schemaCategory.name, schema);
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState
    ) {
        super(changeDetector, { filtered: ImmutableArray.empty(), isOpen: true });
    }

    public ngOnInit() {
        this.next(s => ({ ...s, isOpen: !this.localStore.getBoolean(this.configKey()) }));
    }

    public toggle() {
        this.next(s => ({ ...s, isOpen: !s.isOpen }));

        this.localStore.setBoolean(this.configKey(), !this.snapshot.isOpen);
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes['schemaCategory'] || changes['schemasFilter']) {
            let filtered = this.schemaCategory.schemas;

            if (this.forContent) {
                filtered = filtered.filter(x => x.canReadContents && x.isPublished);
            }

            let isOpen = false;

            if (this.schemasFilter) {
                filtered = filtered.filter(x => x.name.indexOf(this.schemasFilter) >= 0);

                isOpen = true;
            } else {
                isOpen = this.localStore.get(`schema-category.${this.schemaCategory.name}`) !== 'false';
            }

            this.next(s => ({ ...s, isOpen, filtered }));
        }
    }

    public schemaRoute(schema: SchemaDto) {
        if (schema.isSingleton && this.forContent) {
            return [schema.name, schema.id];
        } else {
            return [schema.name];
        }
    }

    public changeCategory(schema: SchemaDto) {
        this.schemasState.changeCategory(schema, this.schemaCategory.name);
    }

    public emitRemove() {
        this.remove.emit();
    }

    public trackBySchema(index: number, schema: SchemaDto) {
        return schema.id;
    }

    private configKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.closed`;
    }
}
