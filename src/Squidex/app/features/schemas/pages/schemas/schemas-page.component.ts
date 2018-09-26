/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { map, onErrorResumeNext } from 'rxjs/operators';

import {
    AppsState,
    CreateCategoryForm,
    DialogModel,
    MessageBus,
    SchemaDto,
    SchemasState
} from '@app/shared';

import { SchemaCloning } from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent implements OnDestroy, OnInit {
    private schemaCloningSubscription: Subscription;

    public addSchemaDialog = new DialogModel();
    public addCategoryForm = new CreateCategoryForm(this.formBuilder);

    public schemasFilter = new FormControl();

    public import: any;

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router
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

        this.route.params.pipe(map(q => q['showDialog']))
            .subscribe(showDialog => {
                if (showDialog) {
                    this.addSchemaDialog.show();
                }
            });

        this.schemasState.load().pipe(onErrorResumeNext()).subscribe();
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
                this.addCategoryForm.submitCompleted({});
            }
        }
    }

    public onSchemaCreated(schema: SchemaDto) {
        this.router.navigate([schema.name], { relativeTo: this.route });

        this.addSchemaDialog.hide();
    }

    public createSchema(importing: any = null) {
        this.import = importing;

        this.addSchemaDialog.show();
    }

    public trackByCategory(index: number, category: string) {
        return category;
    }
}

