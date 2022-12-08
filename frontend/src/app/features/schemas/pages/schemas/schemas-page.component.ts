/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { CreateCategoryForm, DialogModel, getCategoryTree, MessageBus, ResourceOwner, SchemaCategory, SchemaDto, SchemasState, value$ } from '@app/shared';
import { SchemaCloning } from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
})
export class SchemasPageComponent extends ResourceOwner implements OnInit {
    public addSchemaDialog = new DialogModel();
    public addCategoryForm = new CreateCategoryForm();

    public schemasFilter = new UntypedFormControl();

    public categories =
        combineLatest([
            value$(this.schemasFilter),
            this.schemasState.schemas,
            this.schemasState.addedCategories,
        ], (filter, schemas, categories) => {
            return getCategoryTree(schemas, categories, filter);
        });

    public import: any;

    constructor(
        public readonly schemasState: SchemasState,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.messageBus.of(SchemaCloning)
                .subscribe(event => {
                    this.import = event.schema;

                    this.addSchemaDialog.show();
                }));

        this.own(
            this.route.params.pipe(map(q => q['showDialog']))
                .subscribe(showDialog => {
                    if (showDialog) {
                        this.addSchemaDialog.show();
                    }
                }));
    }

    public removeCategory(name: string) {
        this.schemasState.removeCategory(name);
    }

    public addCategory() {
        const value = this.addCategoryForm.submit();

        if (value) {
            try {
                this.schemasState.addCategory(value.name);
            } finally {
                this.addCategoryForm.submitCompleted();
            }
        }
    }

    public redirectSchema(schema: SchemaDto) {
        this.router.navigate([schema.name], { relativeTo: this.route });

        this.addSchemaDialog.hide();
    }

    public createSchema(importing: any = null) {
        this.import = importing;

        this.addSchemaDialog.show();
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }
}
