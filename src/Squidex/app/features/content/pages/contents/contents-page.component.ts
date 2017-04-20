/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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
    AuthService,
    ContentDto,
    ContentsService,
    DateTime,
    FieldDto,
    ImmutableArray,
    MessageBus,
    NotificationService,
    Pager,
    SchemaDetailsDto,
    UsersProviderService,
    Version
} from 'shared';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html'
})
export class ContentsPageComponent extends AppComponentBase implements OnDestroy, OnInit {
    private contentCreatedSubscription: Subscription;
    private contentUpdatedSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public contentItems: ImmutableArray<ContentDto>;
    public contentFields: FieldDto[];
    public contentsFilter = new FormControl();
    public contentsQuery = '';
    public contentsPager = new Pager(0);

    public languages: AppLanguageDto[] = [];
    public languageSelected: AppLanguageDto;

    public get columnWidth() {
        return 100 / this.contentFields.length;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly authService: AuthService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
        super(notifications, users, apps);
    }

    public ngOnDestroy() {
        this.contentCreatedSubscription.unsubscribe();
        this.contentUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.contentCreatedSubscription =
            this.messageBus.of(ContentCreated)
                .subscribe(message => {
                    this.contentItems = this.contentItems.pushFront(this.createContent(message.id, message.data, message.version, message.isPublished));
                    this.contentsPager = this.contentsPager.incrementCount();
                });

        this.contentUpdatedSubscription =
            this.messageBus.of(ContentUpdated)
                .subscribe(message => {
                    this.updateContents(message.id, undefined, message.data, message.version);
                });

        this.route.data.map(p => p['appLanguages'])
            .subscribe((languages: AppLanguageDto[]) => {
                this.languages = languages;
            });

        this.route.data.map(p => p['schema'])
            .subscribe(schema => {
                this.schema = schema;

                this.reset();
                this.load();
            });
    }

    public search() {
        this.contentsQuery = this.contentsFilter.value;
        this.contentsPager = new Pager(0);

        this.load();
    }

    private reset() {
        this.contentItems = ImmutableArray.empty<ContentDto>();
        this.contentsFilter.setValue('');
        this.contentsPager = new Pager(0);

        this.loadFields();
    }

    public publishContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.publishContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.updateContents(content.id, true, content.data, content.version.value);
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublishContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.unpublishContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.updateContents(content.id, false, content.data, content.version.value);
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.deleteContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.contentItems = this.contentItems.removeAll(x => x.id === content.id);
                this.contentsPager = this.contentsPager.decrementCount();

                this.messageBus.publish(new ContentDeleted(content.id));
            }, error => {
                this.notifyError(error);
            });
    }

    public selectLanguage(language: AppLanguageDto) {
        this.languageSelected = language;
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }
    }

    private load() {
        this.appName()
            .switchMap(app => this.contentsService.getContents(app, this.schema.name, this.contentsPager.pageSize, this.contentsPager.skip, this.contentsQuery))
               .subscribe(dtos => {
                    this.contentItems = ImmutableArray.of(dtos.items);

                    this.contentsPager = this.contentsPager.setCount(dtos.total);
                }, error => {
                    this.notifyError(error);
                });
    }

    public goNext() {
        this.contentsPager = this.contentsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.contentsPager = this.contentsPager.goPrev();

        this.load();
    }

    private updateContents(id: string, p: boolean | undefined, data: any, version: string) {
        this.contentItems = this.contentItems.replaceAll(x => x.id === id, c => this.updateContent(c, p === undefined ? c.isPublished : p, data, version));
    }

    private createContent(id: string, data: any, version: string, isPublished: boolean): ContentDto {
        const me = `subject:${this.authService.user!.id}`;

        const newContent =
            new ContentDto(
                id,
                isPublished,
                me, me,
                DateTime.now(),
                DateTime.now(),
                data,
                new Version(version));

        return newContent;
    }

    private updateContent(content: ContentDto, isPublished: boolean, data: any, version: string): ContentDto {
        const me = `subject:${this.authService.user!.id}`;

        const newContent =
            new ContentDto(
                content.id,
                isPublished,
                content.createdBy, me,
                content.created, DateTime.now(),
                data,
                new Version(version));

        return newContent;
    }
}

