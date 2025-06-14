/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs/operators';
import { ConfirmClickDirective, defined, DropdownMenuComponent, LayoutComponent, ListViewComponent, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, SchemasState, SidebarMenuDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UIOptions } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaEditFormComponent } from './common/schema-edit-form.component';
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
export class SchemaPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public readonly hideIndexes = inject(UIOptions).value.hideIndexes;

    public schema!: SchemaDto;
    public schemaTab = this.route.queryParams.pipe(map(x => x['tab'] || 'fields'));

    public editOptionsDropdown = new ModalModel();

    constructor(
        public readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.schemasState.selectedSchema.pipe(defined())
                .subscribe(schema => {
                    this.schema = schema;
                }));
    }

    public cloneSchema() {
        this.messageBus.emit(new SchemaCloning(this.schema.export()));
    }

    public publish() {
        this.schemasState.publish(this.schema).subscribe();
    }

    public unpublish() {
        this.schemasState.unpublish(this.schema).subscribe();
    }

    public deleteSchema() {
        this.schemasState.delete(this.schema)
            .subscribe(() => {
                this.back();
            });
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}
