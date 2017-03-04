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
    private messageCreatedSubscription: Subscription;
    private messageUpdatedSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public contentItems: ImmutableArray<ContentDto>;
    public contentFields: FieldDto[];
    public contentTotal = 0;

    public languages: AppLanguageDto[] = [];
    public languageSelected: AppLanguageDto;

    public contentsFilter = new FormControl();

    public pageSize = 10;

    public canGoNext = false;
    public canGoPrev = false;

    public itemFirst = 0;
    public itemLast = 0;

    public currentPage = 0;
    public currentQuery = '';

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
        this.messageCreatedSubscription.unsubscribe();
        this.messageUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.messageCreatedSubscription =
            this.messageBus.of(ContentCreated).subscribe(message => {
                this.itemLast++;
                this.contentTotal++;
                this.contentItems = this.contentItems.pushFront(this.createContent(message.id, message.data, message.version));
            });

        this.messageUpdatedSubscription =
            this.messageBus.of(ContentUpdated).subscribe(message => {
                this.updateContents(message.id, undefined, message.data, message.version);
            });

        this.route.data.map(p => p['appLanguages']).subscribe((languages: AppLanguageDto[]) => {
            this.languages = languages;
        });

        this.route.data.map(p => p['schema']).subscribe(schema => {
            this.schema = schema;

            this.reset();
            this.load();
        });
    }

    public search() {
        this.currentPage = 0;
        this.currentQuery = this.contentsFilter.value;

        this.load();
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

                this.messageBus.publish(new ContentDeleted(content.id));
            }, error => {
                this.notifyError(error);
            });
    }

    public selectLanguage(language: AppLanguageDto) {
        this.languageSelected = language;
    }

    private reset() {
        this.loadFields();

        this.currentPage = 0;
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }
    }

    private load() {
        this.appName()
            .switchMap(app => this.contentsService.getContents(app, this.schema.name, this.pageSize, this.currentPage * this.pageSize, this.currentQuery))
               .subscribe(dtos => {
                    this.contentItems = ImmutableArray.of(dtos.items);
                    this.contentTotal = dtos.total;

                    this.updatePaging();
                }, error => {
                    this.notifyError(error);
                });
    }

    public goNext() {
        if (this.canGoNext) {
            this.currentPage++;

            this.updatePaging();
            this.load();
        }
    }

    public goPrev() {
        if (this.canGoPrev) {
            this.currentPage--;

            this.updatePaging();
            this.load();
        }
    }

    private updatePaging() {
        const totalPages = Math.ceil(this.contentTotal / this.pageSize);

        this.itemFirst = this.currentPage * this.pageSize + 1;
        this.itemLast = Math.min(this.contentTotal, (this.currentPage + 1) * this.pageSize);

        this.canGoNext = this.currentPage < totalPages - 1;
        this.canGoPrev = this.currentPage > 0;
    }

    private updateContents(id: string, p: boolean | undefined, data: any, version: string) {
        this.contentItems = this.contentItems.replaceAll(x => x.id === id, c => this.updateContent(c, p === undefined ? c.isPublished : p, data, version));
    }

    private createContent(id: string, data: any, version: string): ContentDto {
        const me = `subject:${this.authService.user!.id}`;

        const newContent =
            new ContentDto(
                id, false,
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
                content.id, isPublished,
                content.createdBy, me,
                content.created, DateTime.now(),
                data,
                new Version(version));

        return newContent;
    }
}

