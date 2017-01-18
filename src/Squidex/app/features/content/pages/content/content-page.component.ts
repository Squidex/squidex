/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { ContentAdded } from './../messages';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    ContentsService,
    MessageBus,
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

    public languages: AppLanguageDto[] = [];

    public isNewMode = false;

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.route.params.map(p => p['contentId']).subscribe(contentId => {
            this.isNewMode = !contentId;
        });

        this.route.data.map(p => p['appLanguages']).subscribe((languages: AppLanguageDto[]) => {
            this.languages = languages;
        });

        this.route.data.map(p => p['schema']).subscribe((schema: SchemaDetailsDto) => {
            this.schema = schema;

            this.setupForm(schema);
        });
    }

    public saveContent() {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.disable();

            const data = this.contentForm.value;

            this.appName()
                .switchMap(app => this.contentsService.postContent(app, this.schema.name, data))
                    .subscribe(() => {
                        this.reset();
                        this.messageBus.publish(new ContentAdded());
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
        }
    }

    public reset() {
        this.enable();

        this.contentForm.reset();
        this.contentFormSubmitted = false;
    }

    public enable() {
        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            const fieldForm = this.contentForm.controls[field.name];

            fieldForm.enable();
        }
    }

    public disable() {
        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            const fieldForm = this.contentForm.controls[field.name];

            fieldForm.disable();
        }
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

            const group = new FormGroup({});

            if (field.properties.isLocalizable) {
                for (let language of this.languages) {
                    group.addControl(language.iso2Code, new FormControl(undefined, validators));
                }
            } else {
                group.addControl('iv', new FormControl(undefined, validators));
            }

            controls[field.name] = group;
        }

        this.contentForm = new FormGroup(controls);
    }
}

