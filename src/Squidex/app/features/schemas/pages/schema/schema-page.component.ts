/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter, map, onErrorResumeNext } from 'rxjs/operators';

import {
    AppsState,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    MessageBus,
    ModalView,
    PatternsState,
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
export class SchemaPageComponent implements OnDestroy, OnInit {
    private selectedSchemaSubscription: Subscription;

    public fieldTypes = fieldTypes;

    public schemaExport: any;
    public schema: SchemaDetailsDto;

    public exportSchemaDialog = new ModalView();

    public configureScriptsDialog = new ModalView();

    public editOptionsDropdown = new ModalView();
    public editSchemaDialog = new ModalView();

    public addFieldDialog = new ModalView();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
    }

    public ngOnDestroy() {
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.patternsState.load().pipe(onErrorResumeNext()).subscribe();

        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema.pipe(filter(s => !!s), map(s => s!))
                .subscribe(schema => {
                    this.schema = schema;

                    this.export();
                });
    }

    public publish() {
        this.schemasState.publish(this.schema).subscribe();
    }

    public unpublish() {
        this.schemasState.unpublish(this.schema).subscribe();
    }

    public sortFields(fields: FieldDto[]) {
        this.schemasState.sortFields(this.schema, fields).subscribe();
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId;
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

