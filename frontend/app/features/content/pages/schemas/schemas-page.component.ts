/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { LocalStoreService, SchemaCategory, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent implements OnInit {
    public schemasFilter = new FormControl();

    public isCollapsed: boolean;

    public get width() {
        return this.isCollapsed ? '4rem' : '16rem';
    }

    constructor(
        public readonly schemasState: SchemasState,
        private readonly localStore: LocalStoreService
    ) {
        this.isCollapsed = localStore.getBoolean('content.schemas.collapsed');
    }

    public ngOnInit() {
        this.schemasState.load();
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean('content.schemas.collapsed', this.isCollapsed);
    }

    public trackByCategory(index: number, category: SchemaCategory) {
        return category.name;
    }
}