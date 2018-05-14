/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { AppsState, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent implements OnInit {
    public schemasFilter = new FormControl();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.schemasState.load().onErrorResumeNext().subscribe();
    }

    public trackByCategory(index: number, category: string) {
        return category;
    }
}

