/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    AppsState,
    MessageBus,
    ModalView,
    SchemaDto
} from '@app/shared';

import { SchemaCloning } from './../messages';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent implements OnDestroy, OnInit {
    private schemaCloningSubscription: Subscription;

    public addSchemaDialog = new ModalView();

    public schemasFilter = new FormControl();
    public schemasFiltered =
        this.schemasState.schemas
            .combineLatest(this.schemasFilter.valueChanges.startWith(''),
                (schemas, query) => {
                    if (query && query.length > 0) {
                        return schemas.filter(t => t.name.indexOf(query) >= 0);
                    } else {
                        return schemas;
                    }
                });

    public import: any;

    constructor(
        public readonly appsState: AppsState,
        private readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
    }

    public ngOnDestroy() {
        this.schemaCloningSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.schemaCloningSubscription =
            this.messageBus.of(SchemaCloning)
                .subscribe(m => {
                    this.import = m.schema;

                    this.addSchemaDialog.show();
                });

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

    public trackBySchema(index: number, schema: SchemaDto) {
        return schema.id;
    }
}

