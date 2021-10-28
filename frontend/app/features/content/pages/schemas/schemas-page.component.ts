/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { AppsState, getCategoryTree, LocalStoreService, SchemaCategory, SchemasState, Settings, value$ } from '@app/shared';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
})
export class SchemasPageComponent {
    public schemasFilter = new FormControl();

    public schemas =
        this.schemasState.schemas.pipe(
            map(schemas => {
                const app = this.appsState.snapshot.selectedApp!;

                return schemas.filter(schema =>
                    schema.canReadContents &&
                    schema.isPublished &&
                    schema.type !== 'Component' &&
                    !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(schema.name)],
                );
            }));

    public categories =
        combineLatest([
            value$(this.schemasFilter),
            this.schemas,
            this.schemasState.categoryNames,
        ], (filter, schemas, categories) => {
            return getCategoryTree(schemas, categories, filter);
        });

    public isCollapsed: boolean;

    public get width() {
        return this.isCollapsed ? '4rem' : '16rem';
    }

    constructor(
        public readonly schemasState: SchemasState,
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
    ) {
        this.isCollapsed = localStore.getBoolean(Settings.Local.SCHEMAS_COLLAPSED);
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(Settings.Local.SCHEMAS_COLLAPSED, this.isCollapsed);
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }
}
