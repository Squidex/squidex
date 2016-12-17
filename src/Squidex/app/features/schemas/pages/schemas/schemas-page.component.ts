/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    NotificationService,
    SchemaDto,
    SchemasService,
    UsersProviderService
} from 'shared';

const FALLBACK_NAME = 'my-schema';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent extends AppComponentBase {
    public modalDialog = new ModalView();

    public schemasFilter: string;
    public schemas = ImmutableArray.empty<SchemaDto>();

    public get filteredSchemas() {
        let result = this.schemas;

        if (this.schemasFilter && this.schemasFilter.length > 0) {
            result = result.filter(t => t.name.indexOf(this.schemasFilter) >= 0);
        }

        result =
            result.sort((a, b) => {
                if (a.name < b.name) {
                    return -1;
                }
                if (a.name > b.name) {
                    return 1;
                }
                return 0;
            });

        return result;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly formBuilder: FormBuilder,
        private readonly schemasService: SchemasService
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .subscribe(dtos => {
                this.schemas = ImmutableArray.of(dtos);
            }, error => {
                this.notifyError(error);
            });
    }

    public onSchemaCreationCompleted(dto: SchemaDto) {
        this.schemas = this.schemas.push(dto);

        this.modalDialog.hide();
    }
}

