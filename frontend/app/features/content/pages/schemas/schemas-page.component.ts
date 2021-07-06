/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { LocalStoreService, SchemaCategory, SchemasState, Settings } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
})
export class SchemasPageComponent {
    public schemasFilter = new FormControl();

    public isCollapsed: boolean;

    public get width() {
        return this.isCollapsed ? '4rem' : '16rem';
    }

    constructor(
        public readonly schemasState: SchemasState,
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
