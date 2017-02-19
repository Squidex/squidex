/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    ContentDto,
    fadeAnimation,
    FieldDto,
    ModalView,
    NotificationService,
    SchemaDto,
    UsersProviderService
} from 'shared';

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content-item.component.scss'],
    templateUrl: './content-item.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentItemComponent extends AppComponentBase implements OnInit {
    public dropdown = new ModalView(false, true);

    @Output()
    public publishing = new EventEmitter<ContentDto>();

    @Output()
    public unpublishing = new EventEmitter<ContentDto>();

    @Output()
    public deleting = new EventEmitter<ContentDto>();

    @Input()
    public fields: FieldDto[];

    @Input()
    public language: AppLanguageDto;

    @Input()
    public schema: SchemaDto;

    @Input('sqxContent')
    public content: ContentDto;

    public values: any[] = [];

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService) {
        super(notifications, users, apps);
    }

    public ngOnInit() {
        this.values = [];

        for (let field of this.fields) {
            this.values.push(this.getValue(field));
        }
    }

    public getValue(field: FieldDto): any {
        const contentField = this.content.data[field.name];

        if (!contentField) {
            return '';
        }

        if (field.properties.isLocalizable) {
            return contentField[this.language.iso2Code];
        } else {
            return contentField['iv'];
        }
    }
}

