/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { ContentChanged } from './../messages';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    ContentDto,
    ContentsDto,
    ContentsService,
    FieldDto,
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
export class ContentsPageComponent extends AppComponentBase implements OnInit {
    private messageSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public contents: ContentsDto;
    public contentFields: FieldDto[];

    public languages: AppLanguageDto[] = [];

    public selectedLanguage: AppLanguageDto;

    public page = 0;
    public query = '';

    public get columnWidth() {
        return 100 / this.contentFields.length;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus
    ) {
        super(apps, notifications, users);
    }
    public selectLanguage(language: AppLanguageDto) {
        this.selectedLanguage = language;
    }

    public ngOnInit() {
        this.messageSubscription =
            this.messageBus.of(ContentChanged).delay(2000).subscribe(message => {
                this.load();
            });

        this.route.data.map(p => p['appLanguages']).subscribe((languages: AppLanguageDto[]) => {
            this.languages = languages;

            this.selectedLanguage = languages.filter(t => t.isMasterLanguage)[0];
        });

        this.route.data.map(p => p['schema']).subscribe(schema => {
            this.schema = schema;

            this.reset();
            this.loadFields();
            this.load();
        });
    }

    public getFieldContent(content: ContentDto, field: FieldDto): any {
        const contentField = content.data[field.name];

        if (!contentField) {
            return '';
        }

        if (field.properties.isLocalizable) {
            return contentField[this.selectedLanguage.iso2Code];
        } else {
            return contentField['iv'];
        }
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
                    this.contents = dtos;
                }, error => {
                    this.notifyError(error);
                });
    }
}

