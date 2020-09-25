/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { fadeAnimation, LocalStoreService, SchemaCategory, SchemaDto, SchemasList, SchemasState } from '@app/shared/internal';
import { AppsState } from '../state/apps.state';
import { Settings } from '../state/settings';

@Component({
    selector: 'sqx-schema-category',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SchemaCategoryComponent implements OnChanges {
    @Output()
    public remove = new EventEmitter();

    @Input()
    public schemaCategory: SchemaCategory;

    @Input()
    public schemasFilter: string;

    @Input()
    public forContent: boolean;

    public filteredSchemas: SchemasList;

    public isCollapsed = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState
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

            this.filteredSchemas = this.filteredSchemas.filter(x => x.canReadContents && x.isPublished);
            this.filteredSchemas = this.filteredSchemas.filter(x => !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(x.name)]);
        }

        if (this.schemasFilter) {
            this.filteredSchemas = this.filteredSchemas.filter(x => x.name.indexOf(this.schemasFilter) >= 0);

            this.isCollapsed = false;
        } else {
            this.isCollapsed = this.localStore.getBoolean(this.configKey());
        }
    }

    public schemaRoute(schema: SchemaDto) {
        if (schema.isSingleton && this.forContent) {
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

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }

    private configKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.collapsed`;
    }
}
