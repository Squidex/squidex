/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import {
    AppComponentBase,
    AppsStoreService,
    NotificationService,
    SchemaDetailsDto,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent extends AppComponentBase {
    public schema: SchemaDetailsDto;

    public contentForm: FormGroup;

    public isNewMode = false;

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly route: ActivatedRoute
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.route.params.map(p => p['contentId']).subscribe(contentId => {
            this.isNewMode = !contentId;
        });

        this.route.data.map(p => p['schema']).subscribe((schema: SchemaDetailsDto) => {
            this.schema = schema;

            this.setupForm(schema);
        });
    }

    private setupForm(schema: SchemaDetailsDto) {
        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const formControl = new FormControl();

            controls[field.name] = formControl;
        }

        this.contentForm = new FormGroup(controls);
    }
}

