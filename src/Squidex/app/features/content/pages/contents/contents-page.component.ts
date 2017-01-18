/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
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
    UsersProviderService
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

    public page = 0;
    public query = '';

    public get columnWidth() {
        return 100 / this.contentFields.length;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly authService: AuthService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
        super(apps, notifications, users);
    }

    public ngOnDestroy() {
        this.messageCreatedSubscription.unsubscribe();
        this.messageUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.messageUpdatedSubscription =
            this.messageBus.of(ContentUpdated).subscribe(message => {
                this.contentItems = this.contentItems.replaceAll(x => x.id === message.id, c => this.updateContent(c, true, message.data));
            });

        this.messageCreatedSubscription =
            this.messageBus.of(ContentCreated).subscribe(message => {
                this.contentTotal++;
                this.contentItems = this.contentItems.pushFront(this.createContent(message.id, message.data));
            });

        this.route.data.map(p => p['appLanguages']).subscribe((languages: AppLanguageDto[]) => {
            this.languages = languages;
            this.languageSelected = languages.filter(t => t.isMasterLanguage)[0];
        });

        this.route.data.map(p => p['schema']).subscribe(schema => {
            this.schema = schema;

            this.reset();
            this.loadFields();
            this.load();
        });
    }

    public publishContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.publishContent(app, this.schema.name, content.id))
            .subscribe(() => {
                this.contentItems = this.contentItems.replaceAll(x => x.id === content.id, c => this.updateContent(c, true, content.data));
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublishContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.unpublishContent(app, this.schema.name, content.id))
            .subscribe(() => {
                this.contentItems = this.contentItems.replaceAll(x => x.id === content.id, c => this.updateContent(c, false, content.data));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteContent(content: ContentDto) {
        this.appName()
            .switchMap(app => this.contentsService.deleteContent(app, this.schema.name, content.id))
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
        this.page = 0;
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }
    }

    private load() {
        this.appName()
            .switchMap(app => this.contentsService.getContents(app, this.schema.name, 20, this.page * 20, this.query))
               .subscribe(dtos => {
                    this.contentItems = ImmutableArray.of(dtos.items);
                    this.contentTotal = dtos.total;
                }, error => {
                    this.notifyError(error);
                });
    }

    private createContent(id: string, data: any): ContentDto {
        const me = `subject:${this.authService.user!.id}`;

        const newContent =
            new ContentDto(
                id, false,
                me, me,
                DateTime.now(),
                DateTime.now(),
                data);

        return newContent;
    }

    private updateContent(content: ContentDto, isPublished: boolean, data: any): ContentDto {
        const me = `subject:${this.authService.user!.id}`;

        const newContent =
            new ContentDto(
                content.id, isPublished,
                content.createdBy, me,
                content.created, DateTime.now(),
                data);

        return newContent;
    }
}

