/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentDeleted,
    ContentUpdated
} from './../messages';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    ContentDto,
    ContentsService,
    MessageBus,
    NotificationService,
    NumberFieldPropertiesDto,
    SchemaDetailsDto,
    StringFieldPropertiesDto,
    UsersProviderService,
    ValidatorsEx,
    Version
} from 'shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent extends AppComponentBase implements OnDestroy, OnInit {
    private messageSubscription: Subscription;
    private version: Version = new Version('');

    public schema: SchemaDetailsDto;

    public contentFormSubmitted = false;
    public contentForm: FormGroup;
    public contentData: any = null;
    public contentId: string;

    public isNewMode = true;

    public languages: AppLanguageDto[] = [];

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super(notifications, users, apps);
    }

    public ngOnDestroy() {
        this.messageSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.messageSubscription =
            this.messageBus.of(ContentDeleted)
                .subscribe(message => {
                    if (message.id === this.contentId) {
                        this.router.navigate(['../'], { relativeTo: this.route });
                    }
                });

        this.route.parent.data.map(p => p['appLanguages'])
            .subscribe((languages: AppLanguageDto[]) => {
                this.languages = languages;
            });

        this.route.parent.data.map(p => p['schema'])
            .subscribe((schema: SchemaDetailsDto) => {
                this.setupForm(schema);
            });

        this.route.data.map(p => p['content'])
            .subscribe((content: ContentDto) => {
                this.populateForm(content);
            });
    }

    public saveContent() {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.disable();

            const data = this.contentForm.value;

            if (this.isNewMode) {
                this.appName()
                    .switchMap(app => this.contentsService.postContent(app, this.schema.name, data, this.version))
                    .subscribe(created => {
                        this.messageBus.publish(new ContentCreated(created.id, created.data, this.version.value));

                        this.router.navigate(['../'], { relativeTo: this.route });
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
            } else {
                this.appName()
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.contentId, data, this.version))
                    .subscribe(() => {
                        this.messageBus.publish(new ContentUpdated(this.contentId, data, this.version.value));

                        this.router.navigate(['../'], { relativeTo: this.route });
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
            }
        }
    }

    private enable() {
        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            const fieldForm = this.contentForm.controls[field.name];

            fieldForm.enable();
        }
    }

    private disable() {
        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            const fieldForm = this.contentForm.controls[field.name];

            fieldForm.disable();
        }
    }

    private setupForm(schema: SchemaDetailsDto) {
        this.schema = schema;

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

    private populateForm(content: ContentDto) {
        if (!content) {
            this.contentData = undefined;
            this.contentId = undefined;
            this.isNewMode = true;
            return;
        } else {
            this.contentData = content.data;
            this.contentId = content.id;
            this.isNewMode = false;
        }

        for (const field of this.schema.fields) {
            const fieldValue = content.data[field.name] || {};
            const fieldForm = <FormGroup>this.contentForm.controls[field.name];

             if (field.properties.isLocalizable) {
                for (let language of this.languages) {
                    fieldForm.controls[language.iso2Code].setValue(fieldValue[language.iso2Code]);
                }
            } else {
                fieldForm.controls['iv'].setValue(fieldValue['iv']);
            }
        }
    }
}

