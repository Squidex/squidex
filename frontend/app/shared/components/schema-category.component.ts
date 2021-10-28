/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, CdkDragStart } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { fadeAnimation, LocalStoreService, SchemaCategory, SchemaDto, SchemasList, SchemasState } from '@app/shared/internal';
import { AppsState } from '../state/apps.state';
import { Settings } from '../state/settings';

const ITEM_HEIGHT = 2.5;

@Component({
    selector: 'sqx-schema-category[schemaCategory]',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchemaCategoryComponent implements OnChanges {
    @Output()
    public remove = new EventEmitter();

    @Output()
    public schemaCountUpdated = new EventEmitter<number>();

    @Input()
    public schemaCategory: SchemaCategory;

    @Input()
    public schemasFilter?: ((schema: SchemaDto) => boolean) | null;

    @Input()
    public forContent?: boolean | null;

    public filteredSchemas: SchemasList;

    public childSchemaCounts: { [name: string]: number } = {};

    public filteredHierarchicalSchemaCount = 0;

    public isCollapsed = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState,
        private readonly cdr: ChangeDetectorRef
    ) {
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(this.configKey(), this.isCollapsed);
    }

    public ngOnChanges() {
        this.filteredSchemas = this.schemaCategory.schemas;

        if (this.forContent) {
            const app = this.appsState.snapshot.selectedApp!;

            this.filteredSchemas = this.filteredSchemas.filter(x => x.canReadContents && x.isPublished && x.type !== 'Component');
            this.filteredSchemas = this.filteredSchemas.filter(x => !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(x.name)]);
        }

        const filter = this.schemasFilter;

        if (filter) {
            this.filteredSchemas = this.filteredSchemas.filter(x => filter(x));

            this.isCollapsed = false;
        } else {
            this.isCollapsed = this.localStore.getBoolean(this.configKey());
        }

        this.emitSchemaCount();
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
        return `${ITEM_HEIGHT * this.filteredSchemas.length}rem`;
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }

    public childSchemaCountUpdate(categoryName: string, count: number) {
        this.childSchemaCounts[categoryName] = count;
        this.emitSchemaCount();
        this.cdr.detectChanges();
    }

    private emitSchemaCount() {
        // Add up all the filtered counts of the children
        let filtered = this.filteredSchemas.length;

        for (const category of Object.keys(this.childSchemaCounts)) {
            filtered += this.childSchemaCounts[category];
        }

        this.filteredHierarchicalSchemaCount = filtered;
        this.schemaCountUpdated.emit(this.filteredHierarchicalSchemaCount);
    }

    private configKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.collapsed`;
    }
}
