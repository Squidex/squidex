/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    AppPatternDto,
    AppPatternsService,
    AppsState,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    MessageBus,
    ModalView,
    SchemaDetailsDto
} from '@app/shared';

import {
    SchemaCloning
} from './../messages';

import { SchemasState } from './../../state/schemas.state';

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

    public patterns: AppPatternDto[] = [];

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
        private readonly patternsService: AppPatternsService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
    }

    public ngOnDestroy() {
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.patternsService.getPatterns(this.appsState.appName)
            .subscribe(dtos => {
                this.patterns = dtos.patterns;
            });

        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema!;

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
        const result: any = {
            fields: this.schema.fields.map(field => {
                const copy: any = Object.assign({}, field);

                delete copy.fieldId;

                for (const key in copy.properties) {
                    if (copy.properties.hasOwnProperty(key)) {
                        if (!copy.properties[key]) {
                            delete copy.properties[key];
                        }
                    }
                }

                return copy;
            }),
            properties: {}
        };

        if (this.schema.properties.label) {
            result.properties.label = this.schema.properties.label;
        }

        if (this.schema.properties.hints) {
            result.properties.hints = this.schema.properties.hints;
        }

        this.schemaExport = result;
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}

