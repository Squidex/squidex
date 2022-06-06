/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, CdkDragStart } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { LocalStoreService, SchemaCategory, SchemaDto, SchemasState } from '@app/shared/internal';

const ITEM_HEIGHT = 2.5;

@Component({
    selector: 'sqx-schema-category[schemaCategory]',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchemaCategoryComponent implements OnChanges {
    @Output()
    public remove = new EventEmitter<string>();

    @Input()
    public schemaCategory!: SchemaCategory;

    @Input()
    public schemaTarget?: 'Schema' | 'Contents';

    public isCollapsed = false;

    public get forContent() {
        return this.schemaTarget === 'Contents';
    }

    public get schemas() {
        return this.schemaCategory.schemasFiltered;
    }

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState,
    ) {
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(this.isCollapsedKey(), this.isCollapsed);
    }

    public ngOnChanges() {
        if (this.schemaCategory.countSchemasInSubtreeFiltered < this.schemaCategory.countSchemasInSubtree) {
            this.isCollapsed = false;
        } else {
            this.isCollapsed = this.localStore.getBoolean(this.isCollapsedKey());
        }
    }

    public schemaRoute(schema: SchemaDto) {
        if (schema.type === 'Singleton' && this.forContent) {
            return [schema.name, schema.id, 'history'];
        } else {
            return [schema.name];
        }
    }

    public changeCategory(drag: CdkDragDrop<any>) {
        if (drag.previousContainer !== drag.container) {
            this.schemasState.changeCategory(drag.item.data, this.schemaCategory.name);
        }
    }

    public dragStarted(event: CdkDragStart) {
        setTimeout(() => {
            const dropContainer = event.source._dragRef['_dropContainer'];

            if (dropContainer) {
                dropContainer['_cacheOwnPosition']();
                dropContainer['_cacheItemPositions']();
            }
        });
    }

    public getItemHeight() {
        return `${ITEM_HEIGHT}rem`;
    }

    public getContainerHeight() {
        return `${ITEM_HEIGHT * this.schemas.length}rem`;
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }

    private isCollapsedKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.collapsed`;
    }
}
