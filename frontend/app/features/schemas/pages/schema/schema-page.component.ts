/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
    DialogModel,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    MessageBus,
    ModalModel,
    PatternsState,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    sorted
} from '@app/shared';

import {
    SchemaCloning
} from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaPageComponent extends ResourceOwner implements OnInit {
    public fieldTypes = fieldTypes;

    public schema: SchemaDetailsDto;

    public addFieldDialog = new DialogModel();
    public configurePreviewUrlsDialog = new DialogModel();
    public configureScriptsDialog = new DialogModel();
    public editOptionsDropdown = new ModalModel();
    public editSchemaDialog = new DialogModel();
    public exportDialog = new DialogModel();

    public trackByFieldFn: (index: number, field: FieldDto) => any;

    constructor(
        public readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super();

        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnInit() {
        this.patternsState.load();

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;
                }));
    }

    public publish() {
        this.schemasState.publish(this.schema).subscribe();
    }

    public unpublish() {
        this.schemasState.unpublish(this.schema).subscribe();
    }

    public sortFields(event: CdkDragDrop<ReadonlyArray<FieldDto>>) {
        this.schemasState.orderFields(this.schema, sorted(event)).subscribe();
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }

    public deleteSchema() {
        this.schemasState.delete(this.schema)
            .subscribe(() => {
                this.back();
            });
    }

    public cloneSchema() {
        this.messageBus.emit(new SchemaCloning(this.schema.export()));
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}