/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentPublished,
    ContentRemoved,
    ContentUnpublished,
    ContentUpdated,
    ContentVersionSelected
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
    DialogService,
    MessageBus,
    SchemaDetailsDto,
    Version
} from 'shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent extends AppComponentBase implements CanComponentDeactivate, OnDestroy, OnInit {
    private contentPublishedSubscription: Subscription;
    private contentUnpublishedSubscription: Subscription;
    private contentDeletedSubscription: Subscription;
    private contentVersionSelectedSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentFormSubmitted = false;
    public contentForm: FormGroup;

    public isNewMode = true;

    public languages: AppLanguageDto[] = [];

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnDestroy() {
        this.contentVersionSelectedSubscription.unsubscribe();
        this.contentUnpublishedSubscription.unsubscribe();
        this.contentPublishedSubscription.unsubscribe();
        this.contentDeletedSubscription.unsubscribe();
    }

    public ngOnInit() {
        const routeData = allData(this.route);

        this.languages = routeData['appLanguages'];

        this.contentVersionSelectedSubscription =
            this.messageBus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version);
                });

        this.contentPublishedSubscription =
            this.messageBus.of(ContentPublished)
                .subscribe(message => {
                    if (this.content && message.content.id === this.content.id) {
                        this.content = this.content.publish(message.content.lastModifiedBy, message.content.version, message.content.lastModified);
                    }
                });

        this.contentUnpublishedSubscription =
            this.messageBus.of(ContentUnpublished)
                .subscribe(message => {
                    if (this.content && message.content.id === this.content.id) {
                        this.content = this.content.unpublish(message.content.lastModifiedBy, message.content.version, message.content.lastModified);
                    }
                });

        this.contentDeletedSubscription =
            this.messageBus.of(ContentRemoved)
                .subscribe(message => {
                    if (this.content && message.content.id === this.content.id) {
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
            return this.dialogs.confirmUnsavedChanges();
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
                    .switchMap(app => this.contentsService.postContent(app, this.schema.name, requestDto, publish))
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
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.content.id, requestDto, this.content.version))
                    .subscribe(dto => {
                        this.content = this.content.update(dto.payload, this.userToken, dto.version);

                        this.emitContentUpdated(this.content);
                        this.notifyInfo('Content saved successfully.');
                        this.enableContentForm();
                        this.populateContentForm();
                    }, error => {
                        this.notifyError(error);
                        this.enableContentForm();
                    });
            }
        } else {
            this.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private loadVersion(version: number) {
        if (!this.isNewMode && this.content) {
            this.appNameOnce()
                .switchMap(app => this.contentsService.getVersionData(app, this.schema.name, this.content.id, new Version(version.toString())))
                .subscribe(dto => {
                    this.content = this.content.setData(dto);

                    this.emitContentUpdated(this.content);
                    this.notifyInfo('Content version loaded successfully.');
                    this.populateContentForm();
                }, error => {
                    this.notifyError(error);
                });
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

        this.isNewMode = !this.content;

        if (!this.isNewMode) {
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

            if (this.content.status === 'Archived') {
                this.contentForm.disable();
            }
        }
    }
}

