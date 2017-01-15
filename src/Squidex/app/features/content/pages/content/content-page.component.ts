/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import {
    AppComponentBase,
    AppsStoreService,
    ContentsService,
    NotificationService,
    NumberFieldPropertiesDto,
    SchemaDetailsDto,
    StringFieldPropertiesDto,
    UsersProviderService,
    ValidatorsEx
} from 'shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent extends AppComponentBase {
    public schema: SchemaDetailsDto;

    public contentFormSubmitted = false;
    public contentForm: FormGroup;

    public languages = ['iv'];

    public isNewMode = false;

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly contentsService: ContentsService,
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

    public saveContent() {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.contentForm.disable();

            const data = this.contentForm.value;

            this.appName()
                .switchMap(app => this.contentsService.postContent(app, this.schema.name, data))
                    .subscribe(() => {
                        this.reset();
                    }, error => {
                        this.contentForm.enable();
                    });
        }
    }

    public reset() {
        this.contentForm.reset();
        this.contentFormSubmitted = false;
    }

    private setupForm(schema: SchemaDetailsDto) {
        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const validators: ValidatorFn[] = [];

            if (field.properties.isRequired) {
                validators.push(Validators.required);
            }
            if (field.properties instanceof NumberFieldPropertiesDto) {
                validators.push(ValidatorsEx.between(field.properties.minValue, field.properties.maxValue));
            }
            if (field.properties instanceof StringFieldPropertiesDto) {
                if (field.properties.minLength) {
                    validators.push(Validators.minLength(field.properties.minLength));
                }
                if (field.properties.maxLength) {
                    validators.push(Validators.maxLength(field.properties.maxLength));
                }
                if (field.properties.pattern) {
                    validators.push(ValidatorsEx.pattern(field.properties.pattern, field.properties.patternMessage));
                }
            }

            const group = new FormGroup({
                'iv': new FormControl(undefined, validators)
            });

            controls[field.name] = group;
        }

        this.contentForm = new FormGroup(controls);
    }
}

