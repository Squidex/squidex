/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, CdkDragStart } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { fadeAnimation, LocalStoreService, SchemaCategory, SchemaDto, SchemasList, SchemasState } from '@app/shared/internal';
import { AppsState } from '../state/apps.state';
import { Settings } from '../state/settings';

const ITEM_HEIGHT = 2.5;

@Component({
    selector: 'sqx-schema-category',
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

    @Input()
    public schemaCategory: SchemaCategory;

    @Input()
    public schemasFilter: string;

    @Input()
    public forContent?: boolean | null;

    public filteredSchemas: SchemasList;

    public isCollapsed = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState,
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

        if (this.schemasFilter) {
            const regex = new RegExp('/[^a-z\d]/gi');
            const terms = this.schemasFilter.trim().split(' ').map(x => x.trim());
            const searchSchemas = this.filteredSchemas.map(schema => ({ schema, values: schema.name.split(regex).concat((schema.properties.label ?? '').split(regex)) }));
            this.filteredSchemas = searchSchemas.filter(search => terms.every(term => search.values.some(value => value.search(new RegExp(term, 'gi')) > -1))).map(search => search.schema);

            this.isCollapsed = false;
        } else {
            this.isCollapsed = this.localStore.getBoolean(this.configKey());
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
        return `${ITEM_HEIGHT * this.filteredSchemas.length}rem`;
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }

    private configKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.collapsed`;
    }
}
