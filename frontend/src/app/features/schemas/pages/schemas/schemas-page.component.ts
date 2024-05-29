/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { CreateCategoryForm, DialogModel, getCategoryTree, LayoutComponent, MessageBus, ModalDirective, SchemaCategoryComponent, SchemaDto, SchemasState, ShortcutDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, value$ } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaFormComponent } from './schema-form.component';

@Component({
    standalone: true,
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    imports: [
        AsyncPipe,
        CdkDropListGroup,
        FormsModule,
        LayoutComponent,
        ModalDirective,
        ReactiveFormsModule,
        RouterOutlet,
        SchemaCategoryComponent,
        SchemaFormComponent,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SchemasPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

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
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.messageBus.of(SchemaCloning)
                .subscribe(event => {
                    this.import = event.schema;

                    this.addSchemaDialog.show();
                }));

        this.subscriptions.add(
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
}
