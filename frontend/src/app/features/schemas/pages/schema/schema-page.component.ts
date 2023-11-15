/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs/operators';
import { ConfirmClickDirective, DropdownMenuComponent, LayoutComponent, ListViewComponent, ModalDirective, ModalPlacementDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { defined, MessageBus, ModalModel, SchemaDto, SchemasState, Subscriptions } from '@app/shared';
import { SchemaCloning } from '../messages';
import { SchemaEditFormComponent } from './common/schema-edit-form.component';
import { SchemaExportFormComponent } from './export/schema-export-form.component';
import { SchemaFieldsComponent } from './fields/schema-fields.component';
import { SchemaPreviewUrlsFormComponent } from './preview/schema-preview-urls-form.component';
import { SchemaFieldRulesFormComponent } from './rules/schema-field-rules-form.component';
import { SchemaScriptsFormComponent } from './scripts/schema-scripts-form.component';
import { SchemaUIFormComponent } from './ui/schema-ui-form.component';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        NgIf,
        LayoutComponent,
        TooltipDirective,
        RouterLink,
        TourStepDirective,
        ModalDirective,
        DropdownMenuComponent,
        ModalPlacementDirective,
        ConfirmClickDirective,
        NgSwitch,
        NgSwitchCase,
        SchemaUIFormComponent,
        SchemaScriptsFormComponent,
        SchemaExportFormComponent,
        ListViewComponent,
        SchemaPreviewUrlsFormComponent,
        SchemaFieldRulesFormComponent,
        SchemaEditFormComponent,
        NgSwitchDefault,
        SchemaFieldsComponent,
        SidebarMenuDirective,
        RouterLinkActive,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class SchemaPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public readonly exact = { exact: true };

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
