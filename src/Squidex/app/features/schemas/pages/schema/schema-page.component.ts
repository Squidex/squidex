/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
    AppsState,
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
    Types
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

    public schemaExport: any;
    public schema: SchemaDetailsDto;

    public exportSchemaDialog = new DialogModel();

    public configureScriptsDialog = new DialogModel();
    public configurePreviewUrlsDialog = new DialogModel();

    public editOptionsDropdown = new ModalModel();
    public editSchemaDialog = new DialogModel();

    public addFieldDialog = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super();
    }

    public ngOnInit() {
        this.patternsState.load();

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    if (schema) {
                        this.schema = schema;

                        this.export();
                    }
                }));
    }

    public publish() {
        this.schemasState.publish(this.schema).subscribe();
    }

    public unpublish() {
        this.schemasState.unpublish(this.schema).subscribe();
    }

    public sortFields(fields: FieldDto[]) {
        this.schemasState.orderFields(this.schema, fields).subscribe();
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
        this.messageBus.emit(new SchemaCloning(this.schemaExport));
    }

    private export() {
        const cleanup = (source: any, ...exclude: string[]): any => {
            const clone = {};

            for (const key in source) {
                if (source.hasOwnProperty(key) && exclude.indexOf(key) < 0) {
                    const value = source[key];

                    if (value) {
                        clone[key] = value;
                    }
                }
            }

            return clone;
        };

        const result: any = {
            fields: this.schema.fields.map(field => {
                const copy = cleanup(field, 'fieldId');

                copy.properties = cleanup(field.properties);

                if (Types.isArray(copy.nested)) {
                    if (copy.nested.length === 0) {
                        delete copy['nested'];
                    } else {
                        copy.nested = field.nested.map(nestedField => {
                            const nestedCopy = cleanup(nestedField, 'fieldId', 'parentId');

                            nestedCopy.properties = cleanup(nestedField.properties);

                            return nestedCopy;
                        });
                    }
                }

                return copy;
            }),
            properties: cleanup(this.schema.properties)
        };

        this.schemaExport = result;
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}

