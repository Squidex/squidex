/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';

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
export class ContentItemComponent extends AppComponentBase {
    public dropdown = new ModalView(false, true);

    @Output()
    public published = new EventEmitter<ContentDto>();

    @Output()
    public unpublished = new EventEmitter<ContentDto>();

    @Output()
    public deleted = new EventEmitter<ContentDto>();

    @Input()
    public fields: FieldDto[];

    @Input()
    public language: AppLanguageDto;

    @Input()
    public schema: SchemaDto;

    @Input('sqxContent')
    public content: ContentDto;

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService) {
        super(apps, notifications, users);
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

