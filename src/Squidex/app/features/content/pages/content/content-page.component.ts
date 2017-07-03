/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
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
    allData,
    CanComponentDeactivate,
    ContentDto,
    ContentsService,
    fadeAnimation,
    ModalView,
    MessageBus,
    NotificationService,
    SchemaDetailsDto,
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
        const routeData = allData(this.route);

        this.languages = routeData['appLanguages'];

        this.contentDeletedSubscription =
            this.messageBus.of(ContentDeleted)
                .subscribe(message => {
                    if (message.id === this.contentId) {
                        this.router.navigate(['../'], { relativeTo: this.route });
                    }
                });

        this.setupForm(routeData['schema']);

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

            const requestDto = this.contentForm.value;

            const back = () => {
                this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
            };

            if (this.isNewMode) {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.postContent(app, this.schema.name, requestDto, publish, this.version))
                    .subscribe(created => {
                        this.messageBus.publish(new ContentCreated(created.id, created.data, this.version.value, publish));

                        this.notifyInfo('Content created successfully.');
                        back();
                    }, error => {
                        this.notifyError(error);
                        this.enable();
                    });
            } else {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.contentId!, requestDto, this.version))
                    .subscribe(() => {
                        this.messageBus.publish(new ContentUpdated(this.contentId!, requestDto, this.version.value));

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

    private enable() {
        this.contentForm.markAsPristine();

        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            this.contentForm.controls[field.name].enable();
        }
    }

    private disable() {
        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            this.contentForm.controls[field.name].disable();
        }
    }

    private setupForm(schema: SchemaDetailsDto) {
        this.schema = schema;

        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const group = new FormGroup({});

            if (field.partitioning === 'language') {
                for (let language of this.languages) {
                    group.addControl(language.iso2Code, new FormControl(undefined, field.createValidators()));
                }
            } else {
                group.addControl('iv', new FormControl(undefined, field.createValidators()));
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
            const fieldForm = <FormGroup>this.contentForm.get(field.name);

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

