/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDropListGroup } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { CreateCategoryForm, DialogModel, getCategoryTree, LayoutComponent, MessageBus, ModalDirective, routeParam, SchemaCategoryComponent, SchemaDto, SchemasState, ShortcutDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaFormComponent } from './schema-form.component';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDropListGroup,
        FormField,
        FormsModule,
        LayoutComponent,
        ModalDirective,
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
    private readonly showDialog = routeParam('showDialog');

    protected readonly schemasState = inject(SchemasState);

    protected readonly addSchemaDialog = new DialogModel();

    protected readonly addCategoryForm = new CreateCategoryForm();

    protected readonly schemasFilter = form(signal(''));

    protected readonly categories = computed(() => {
        return getCategoryTree(this.schemasState.schemas(), this.schemasState.addedCategories(), this.schemasFilter().value());
    });

    protected readonly source = signal<any>(null);

    constructor() {
        const messageBus = inject(MessageBus);

        effect(onCleanup => {
            const subscription =
                messageBus.of(SchemaCloning)
                    .subscribe(event => {
                        this.source.set(event.schema);

                        this.addSchemaDialog.show();
                    });

            onCleanup(() => subscription.unsubscribe());
        });

        effect(() => {
            if (this.showDialog()) {
                untracked(() => this.addSchemaDialog.show());
            }
        });
    }

    protected removeCategory(name: string) {
        this.schemasState.removeCategory(name);
    }

    protected addCategory() {
        this.addCategoryForm.submit(async value => {
            this.schemasState.addCategory(value.name);
        }, { resetAfterSubmit: true });
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
