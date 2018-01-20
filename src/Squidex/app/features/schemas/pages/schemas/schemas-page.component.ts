/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppContext,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    SchemaDto,
    SchemasService
} from 'shared';

import {
    SchemaCloning,
    SchemaCreated,
    SchemaDeleted,
    SchemaUpdated
} from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent implements OnDestroy, OnInit {
    private schemaUpdatedSubscription: Subscription;
    private schemaDeletedSubscription: Subscription;
    private schemaCloningSubscription: Subscription;

    public addSchemaDialog = new ModalView();

    public schemas = ImmutableArray.empty<SchemaDto>();
    public schemaQuery: string;
    public schemasFilter = new FormControl();
    public schemasFiltered = ImmutableArray.empty<SchemaDto>();

    public import: any;

    constructor(public readonly ctx: AppContext,
        private readonly router: Router,
        private readonly schemasService: SchemasService
    ) {
    }

    public ngOnDestroy() {
        this.schemaUpdatedSubscription.unsubscribe();
        this.schemaDeletedSubscription.unsubscribe();
        this.schemaCloningSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.schemasFilter.valueChanges
            .distinctUntilChanged()
            .debounceTime(100)
            .subscribe(q => {
                this.updateSchemas(this.schemas, this.schemaQuery = q);
            });

        this.ctx.route.params.map(q => q['showDialog'])
            .subscribe(showDialog => {
                if (showDialog) {
                    this.addSchemaDialog.show();
                }
            });

        this.schemaCloningSubscription =
            this.ctx.bus.of(SchemaCloning)
                .subscribe(m => {
                    this.createSchema(m.importing);
                });

        this.schemaUpdatedSubscription =
            this.ctx.bus.of(SchemaUpdated)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.replaceBy('id', m.schema));
                });

        this.schemaDeletedSubscription =
            this.ctx.bus.of(SchemaDeleted)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.filter(s => s.id !== m.schema.id));
                });

        this.load();
    }

    public createSchema(importing: any) {
        this.import = importing;

        this.addSchemaDialog.show();
    }

    private load() {
        this.schemasService.getSchemas(this.ctx.appName)
            .subscribe(dtos => {
                this.updateSchemas(ImmutableArray.of(dtos));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public onSchemaCreated(schema: SchemaDto) {
        this.updateSchemas(this.schemas.push(schema), this.schemaQuery);
        this.emitSchemaCreated(schema);

        this.addSchemaDialog.hide();

        this.router.navigate([ schema.name ], { relativeTo: this.ctx.route });
    }

    private emitSchemaCreated(schema: SchemaDto) {
        this.ctx.bus.emit(new SchemaCreated(schema));
    }

    private updateSchemas(schemas: ImmutableArray<SchemaDto>, query?: string) {
        this.schemas = schemas;

        query = query || this.schemaQuery;

        if (query && query.length > 0) {
            schemas = schemas.filter(t => t.name.indexOf(query!) >= 0);
        }

        this.schemasFiltered = schemas.sortByStringAsc(x => x.name);
    }
}

