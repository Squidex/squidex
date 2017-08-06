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
    allData,
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    AuthService,
    ContentDto,
    ContentsService,
    FieldDto,
    ImmutableArray,
    MessageBus,
    NotificationService,
    Pager,
    SchemaDetailsDto
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
    public languageParameter: string;

    public isReadOnly = false;

    public columnWidth: number;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly authService: AuthService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
        super(notifications, apps);
    }

    public ngOnDestroy() {
        this.contentCreatedSubscription.unsubscribe();
        this.contentUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        const routeData = allData(this.route);

        this.languages = routeData['appLanguages'];

        this.contentCreatedSubscription =
            this.messageBus.of(ContentCreated)
                .subscribe(message => {
                    this.contentItems = this.contentItems.pushFront(message.content);
                    this.contentsPager = this.contentsPager.incrementCount();
                });

        this.contentUpdatedSubscription =
            this.messageBus.of(ContentUpdated)
                .subscribe(message => {
                    this.contentItems = this.contentItems.replaceBy('id', message.content, (o, n) => o.update(n.data, n.lastModifiedBy));
                });

        this.route.params.map(p => <string> p['language'])
            .subscribe(language => {
                this.languageSelected = this.languages.find(l => l.iso2Code === language) || this.languages.find(l => l.isMaster) || this.languages[0];
            });

        this.route.data.map(p => p['schemaOverride'] || p['schema'])
            .subscribe(schema => {
                this.schema = schema;

                this.resetContents();
                this.load();
            });

        this.isReadOnly = routeData['isReadOnly'];
    }

    public dropData(content: ContentDto) {
        return { content, schemaId: this.schema.id };
    }

    public search() {
        this.contentsQuery = this.contentsFilter.value;
        this.contentsPager = new Pager(0);

        this.load();
    }

    public publishContent(content: ContentDto) {
        this.appNameOnce()
            .switchMap(app => this.contentsService.publishContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.contentItems = this.contentItems.replaceBy('id', content.publish(this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublishContent(content: ContentDto) {
        this.appNameOnce()
            .switchMap(app => this.contentsService.unpublishContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.contentItems = this.contentItems.replaceBy('id', content.unpublish(this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteContent(content: ContentDto) {
        this.appNameOnce()
            .switchMap(app => this.contentsService.deleteContent(app, this.schema.name, content.id, content.version))
            .subscribe(() => {
                this.contentItems = this.contentItems.removeAll(x => x.id === content.id);
                this.contentsPager = this.contentsPager.decrementCount();

                this.emitContentDeleted(content);
            }, error => {
                this.notifyError(error);
            });
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app => this.contentsService.getContents(app, this.schema.name, this.contentsPager.pageSize, this.contentsPager.skip, this.contentsQuery))
            .subscribe(dtos => {
                this.contentItems = ImmutableArray.of(dtos.items);
                this.contentsPager = this.contentsPager.setCount(dtos.total);

                if (showInfo) {
                    this.notifyInfo('Contents reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public selectLanguage(language: AppLanguageDto) {
        this.languageSelected = language;
    }

    public goNext() {
        this.contentsPager = this.contentsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.contentsPager = this.contentsPager.goPrev();

        this.load();
    }

    private emitContentDeleted(content: ContentDto) {
        this.messageBus.emit(new ContentDeleted(content));
    }

    private resetContents() {
        this.contentItems = ImmutableArray.empty<ContentDto>();
        this.contentsQuery = '';
        this.contentsFilter.setValue('');
        this.contentsPager = new Pager(0);

        this.loadFields();
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }

        if (this.contentFields.length > 0) {
            this.columnWidth = 100 / this.contentFields.length;
        } else {
            this.columnWidth = 100;
        }
    }
}

