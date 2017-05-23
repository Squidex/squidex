/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subject, Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentDeleted,
    ContentUpdated
} from './../messages';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    CanComponentDeactivate,
    ContentDto,
    ContentsService,
    fadeAnimation,
    ModalView,
    MessageBus,
    NotificationService,
    NumberFieldPropertiesDto,
    SchemaDetailsDto,
    StringFieldPropertiesDto,
    ValidatorsEx,
    Version
} from 'shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentPageComponent extends AppComponentBase implements CanComponentDeactivate, OnDestroy, OnInit {
    private contentDeletedSubscription: Subscription;
    private version: Version = new Version('');
    private cancelPromise: Subject<boolean> | null = null;

    public schema: SchemaDetailsDto;

    public cancelDialog = new ModalView();
    public contentFormSubmitted = false;
    public contentForm: FormGroup;
    public contentData: any = null;
    public contentId: string | null = null;

    public isNewMode = true;

    public languages: AppLanguageDto[] = [];

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super(notifications, apps);
    }

    public ngOnDestroy() {
        this.contentDeletedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.contentDeletedSubscription =
            this.messageBus.of(ContentDeleted)
                .subscribe(message => {
                    if (message.id === this.contentId) {
                        this.router.navigate(['../'], { relativeTo: this.route });
                    }
                });

        this.route.parent!.data.map(p => p['appLanguages'])
            .subscribe((languages: AppLanguageDto[]) => {
                this.languages = languages;
            });

        this.route.parent!.data.map(p => p['schema'])
            .subscribe((schema: SchemaDetailsDto) => {
                this.setupForm(schema);
            });

        this.route.data.map(p => p['content'])
            .subscribe((content: ContentDto) => {
                this.populateForm(content);
            });
    }

    public canDeactivate(): Observable<boolean> | Promise<boolean> | boolean {
        if (!this.contentForm.dirty) {
            return true;
        } else {
            this.cancelDialog.show();

            return this.cancelPromise = new Subject<boolean>();
        }
    }

    public confirmLeave() {
        this.cancelDialog.hide();

        if (this.cancelPromise) {
            this.cancelPromise.next(true);
            this.cancelPromise = null;
        }
    }

    public cancelLeave() {
        this.cancelDialog.hide();

        if (this.cancelPromise) {
            this.cancelPromise.next(false);
            this.cancelPromise = null;
        }
    }

    public saveAndPublish() {
        this.saveContent(true);
    }

    public saveAsDraft() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.disable();

            const data = this.contentForm.value;

            if (this.isNewMode) {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.postContent(app, this.schema.name, data, publish, this.version))
                    .subscribe(created => {
                        this.contentId = created.id;

                        this.messageBus.publish(new ContentCreated(created.id, created.data, this.version.value, publish));

                        this.notifyInfo('Content created successfully.');
                        this.finish();
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
            } else {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.contentId!, data, this.version))
                    .subscribe(() => {
                        this.messageBus.publish(new ContentUpdated(this.contentId!, data, this.version.value));

                        this.notifyInfo('Content saved successfully.');
                        this.enable();
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
            }
        } else {
            this.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private finish() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }

    private enable() {
        this.contentForm.markAsPristine();

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

            if (field.partitioning === 'language') {
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
        this.contentForm.markAsPristine();

        if (!content) {
            this.contentData = null;
            this.contentId = null;
            this.isNewMode = true;
            return;
        }

        this.contentData = content.data;
        this.contentId = content.id;
        this.version = content.version;
        this.isNewMode = false;

        for (const field of this.schema.fields) {
            const fieldValue = content.data[field.name] || {};
            const fieldForm = <FormGroup>this.contentForm.controls[field.name];

             if (field.partitioning === 'language') {
                for (let language of this.languages) {
                    fieldForm.controls[language.iso2Code].setValue(fieldValue[language.iso2Code]);
                }
            } else {
                fieldForm.controls['iv'].setValue(fieldValue['iv']);
            }
        }
    }
}

