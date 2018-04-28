/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import {
    AppsState,
    SchemaDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent implements OnInit {
    public schemasFilter = new FormControl();
    public schemasFiltered =
        this.schemasState.publishedSchemas
            .combineLatest(this.schemasFilter.valueChanges.startWith(''),
                (schemas, query) => {
                    if (query && query.length > 0) {
                        return schemas.filter(t => t.name.indexOf(query) >= 0);
                    } else {
                        return schemas;
                    }
                });

    constructor(
        public readonly appsState: AppsState,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.schemasState.load().onErrorResumeNext().subscribe();
    }

    public trackBySchema(index: number, schema: SchemaDto) {
        return schema.id;
    }
}

