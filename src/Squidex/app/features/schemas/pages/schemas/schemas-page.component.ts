/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import {
    AppsState,
    fadeAnimation,
    ModalView
} from 'shared';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent implements OnInit {
    public addSchemaDialog = new ModalView();

    public schemasFilter = new FormControl();
    public schemasFiltered =
        this.schemasState.schemasItems
            .combineLatest(this.schemasFilter.valueChanges.startWith(''),
                (schemas, query) => {
                    if (query && query.length > 0) {
                        schemas = schemas.filter(t => t.name.indexOf(query) >= 0);
                    }

                    return schemas.sortByStringAsc(x => x.name);
                });

    public import: any;

    constructor(public readonly appsState: AppsState,
        private readonly route: ActivatedRoute,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.route.params.map(q => q['showDialog'])
            .subscribe(showDialog => {
                if (showDialog) {
                    this.addSchemaDialog.show();
                }
            });

        this.schemasState.load().subscribe();
    }

    public createSchema(importing: any) {
        this.import = importing;

        this.addSchemaDialog.show();
    }
}

