/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateCategoryForm, DialogModel, MessageBus, ResourceOwner, SchemaCategory, SchemaDto, SchemasState } from '@app/shared';
import { map } from 'rxjs/operators';
import { SchemaCloning } from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
})
export class SchemasPageComponent extends ResourceOwner implements OnInit {
    public addSchemaDialog = new DialogModel();
    public addCategoryForm = new CreateCategoryForm(this.formBuilder);

    public schemasFilter = new FormControl();

    public import: any;

    constructor(
        public readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder,
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
