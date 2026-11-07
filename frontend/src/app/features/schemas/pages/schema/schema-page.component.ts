/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs/operators';
import { ConfirmClickDirective, defined, DropdownMenuComponent, LanguagesState, LayoutComponent, ListViewComponent, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, SchemasState, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UIOptions } from '@app/shared';
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
        AsyncPipe,
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
    public readonly hideIndexes = inject(UIOptions).value.hideIndexes;

    public readonly schema;
    public readonly schemaTab;

    public readonly editOptionsDropdown = new ModalModel();

    constructor(
        public readonly schemasState: SchemasState,
        public readonly languageState: LanguagesState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus,
    ) {
        this.schema = toSignal(this.schemasState.selectedSchema.pipe(defined()));

        this.schemaTab = toSignal(
            this.route.queryParams.pipe(map(x => (x['tab'] || 'fields') as string)),
            { initialValue: 'fields' });

        this.languageState.load();
    }

    public cloneSchema() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.messageBus.emit(new SchemaCloning(schema.export()));
    }

    public publish() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.schemasState.publish(schema).subscribe();
    }

    public unpublish() {
        const schema = this.schema();
        if (!schema) {
            return;
        }

        this.schemasState.unpublish(schema).subscribe();
    }

    public deleteSchema() {
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
