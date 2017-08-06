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
    AuthService,
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
    private version = new Version('');
    private cancelPromise: Subject<boolean> | null = null;
    private content: ContentDto;

    public schema: SchemaDetailsDto;

    public cancelDialog = new ModalView();
    public contentFormSubmitted = false;
    public contentForm: FormGroup;
    public contentData: any = null;
    public contentId: string | null = null;

    public isNewMode = true;

    public languages: AppLanguageDto[] = [];

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly authService: AuthService,
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
                    if (message.content.id === this.contentId) {
                        this.router.navigate(['../'], { relativeTo: this.route });
                    }
                });

        this.setupContentForm(routeData['schema']);

        this.route.data.map(p => p['content'])
            .subscribe((content: ContentDto) => {
                this.content = content;

                this.populateContentForm();
            });
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.dirty || this.isNewMode) {
            return Observable.of(true);
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
            this.disableContentForm();

            const requestDto = this.contentForm.value;

            if (this.isNewMode) {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.postContent(app, this.schema.name, requestDto, publish, this.version))
                    .subscribe(dto => {
                        this.content = dto;

                        this.emitContentCreated(this.content);
                        this.notifyInfo('Content created successfully.');
                        this.back();
                    }, error => {
                        this.notifyError(error);
                        this.enableContentForm();
                    });
            } else {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.contentId!, requestDto, this.version))
                    .subscribe(() => {
                        this.content = this.content.update(requestDto, this.authService.user.token);

                        this.emitContentUpdated(this.content);
                        this.notifyInfo('Content saved successfully.');
                        this.enableContentForm();
                    }, error => {
                        this.notifyError(error);
                        this.enableContentForm();
                    });
            }
        } else {
            this.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }

    private emitContentCreated(content: ContentDto) {
        this.messageBus.emit(new ContentCreated(content));
    }

    private emitContentUpdated(content: ContentDto) {
        this.messageBus.emit(new ContentUpdated(content));
    }

    private disableContentForm() {
        this.contentForm.disable();
    }

    private enableContentForm() {
        this.contentForm.markAsPristine();

        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            this.contentForm.controls[field.name].enable();
        }
    }

    private setupContentForm(schema: SchemaDetailsDto) {
        this.schema = schema;

        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const group = new FormGroup({});

            if (field.partitioning === 'language') {
                for (let language of this.languages) {
                    group.addControl(language.iso2Code, new FormControl(undefined, field.createValidators(language.isOptional)));
                }
            } else {
                group.addControl('iv', new FormControl(undefined, field.createValidators(false)));
            }

            controls[field.name] = group;
        }

        this.contentForm = new FormGroup(controls);
    }

    private populateContentForm() {
        this.contentForm.markAsPristine();

        if (!this.content) {
            this.contentData = null;
            this.contentId = null;
            this.isNewMode = true;
            return;
        }

        this.contentData = this.content.data;
        this.contentId = this.content.id;
        this.version = this.content.version;
        this.isNewMode = false;

        for (const field of this.schema.fields) {
            const fieldValue = this.content.data[field.name] || {};
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

