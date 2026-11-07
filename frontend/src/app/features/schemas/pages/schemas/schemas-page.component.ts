/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDropListGroup } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, computed, effect, signal, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
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
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);

    protected readonly schemasState = inject(SchemasState);

    protected readonly addSchemaDialog = new DialogModel();

    protected readonly addCategoryForm = new CreateCategoryForm();

    protected readonly schemasFilter = new UntypedFormControl();

    protected readonly categories;

    protected readonly source = signal<any>(null);

    constructor() {
        const messageBus = inject(MessageBus);

        const schemasFilterValue = toSignal(value$<string>(this.schemasFilter));

        this.categories = computed(() => {
            return getCategoryTree(this.schemasState.schemas(), this.schemasState.addedCategories(), schemasFilterValue());
        });

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

    protected removeCategory(name: string) {
        this.schemasState.removeCategory(name);
    }

    protected addCategory() {
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

    protected redirectSchema(schema: SchemaDto) {
        this.router.navigate([schema.name], { relativeTo: this.route });

        this.addSchemaDialog.hide();
    }

    protected createSchema(source: any = null) {
        this.source.set(source);

        this.addSchemaDialog.show();
    }
}
