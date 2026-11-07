/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { CreateCategoryForm, DialogModel, getCategoryTree, LayoutComponent, MessageBus, ModalDirective, SchemaCategoryComponent, SchemaDto, SchemasState, ShortcutDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, value$ } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaFormComponent } from './schema-form.component';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class SchemasPageComponent {
    public readonly addSchemaDialog = new DialogModel();
    public readonly addCategoryForm = new CreateCategoryForm();

    public readonly schemasFilter = new UntypedFormControl();

    public readonly categories;

    public readonly source = signal<any>(null);

    constructor(messageBus: MessageBus,
        public readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
        this.categories = toSignal(
            combineLatest([
                value$(this.schemasFilter),
                this.schemasState.schemas,
                this.schemasState.addedCategories,
            ], (filter, schemas, categories) => {
                return getCategoryTree(schemas, categories, filter);
            }),
            { initialValue: [] });

        effect(onCleanup => {
            const subscription =
                messageBus.of(SchemaCloning)
                    .subscribe(event => {
                        this.source.set(event.schema);

                        this.addSchemaDialog.show();
                    });

            onCleanup(() => subscription.unsubscribe());
        });

        effect(onCleanup => {
            const subscription =
                this.route.params.pipe(map(q => q['showDialog']))
                    .subscribe(showDialog => {
                        if (showDialog) {
                            this.addSchemaDialog.show();
                        }
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public removeCategory(name: string) {
        this.schemasState.removeCategory(name);
    }

    public addCategory() {
        const value = this.addCategoryForm.submit();
        if (!value) {
            return;
        }

        try {
            this.schemasState.addCategory(value.name);
        } finally {
            this.addCategoryForm.submitCompleted();
        }
    }

    public redirectSchema(schema: SchemaDto) {
        this.router.navigate([schema.name], { relativeTo: this.route });

        this.addSchemaDialog.hide();
    }

    public createSchema(source: any = null) {
        this.source.set(source);

        this.addSchemaDialog.show();
    }
}
