/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmClickDirective, DropdownMenuComponent, LanguagesState, LayoutComponent, ListViewComponent, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, queryParam, SchemasState, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UIOptions } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaEditFormComponent } from './common/schema-edit-form.component';
import { SchemaMigrateFormComponent } from './common/schema-migrate-form.component';
import { SchemaExportFormComponent } from './export/schema-export-form.component';
import { SchemaFieldsComponent } from './fields/schema-fields.component';
import { SchemaIndexesComponent } from './indexes/schema-indexes.component';
import { SchemaPreviewUrlsFormComponent } from './preview/schema-preview-urls-form.component';
import { SchemaFieldRulesFormComponent } from './rules/schema-field-rules-form.component';
import { SchemaScriptsFormComponent } from './scripts/schema-scripts-form.component';
import { SchemaUIFormComponent } from './ui/schema-ui-form.component';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        DropdownMenuComponent,
        LayoutComponent,
        ListViewComponent,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SchemaEditFormComponent,
        SchemaExportFormComponent,
        SchemaFieldRulesFormComponent,
        SchemaFieldsComponent,
        SchemaIndexesComponent,
        SchemaMigrateFormComponent,
        SchemaPreviewUrlsFormComponent,
        SchemaScriptsFormComponent,
        SchemaUIFormComponent,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SchemaPageComponent {
    private readonly messageBus = inject(MessageBus);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly uiOptions = inject(UIOptions);

    protected readonly schemasState = inject(SchemasState);

    protected readonly languageState = inject(LanguagesState);

    protected readonly hideIndexes = this.uiOptions.value.hideIndexes;

    protected readonly schema = this.schemasState.selectedSchema;

    protected readonly schemaTab = queryParam('tab', 'fields');

    protected readonly editOptionsDropdown = new ModalModel();

    constructor() {
        this.languageState.load();
    }

    protected cloneSchema() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.messageBus.emit(new SchemaCloning(schema.export()));
    }

    protected publish() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.schemasState.publish(schema).subscribe();
    }

    protected unpublish() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.schemasState.unpublish(schema).subscribe();
    }

    protected deleteSchema() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.schemasState.delete(schema)
            .subscribe(() => {
                this.back();
            });
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}
