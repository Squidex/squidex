/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    ContentDto,
    DateTime,
    fadeAnimation,
    FieldDto,
    ModalView,
    NotificationService,
    SchemaDto
} from 'shared';

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content-item.component.scss'],
    templateUrl: './content-item.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentItemComponent extends AppComponentBase implements OnInit, OnChanges {
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

    constructor(apps: AppsStoreService, notifications: NotificationService) {
        super(notifications, apps);
    }

    public ngOnChanges() {
        this.updateValues();
    }

    public ngOnInit() {
        this.updateValues();
    }

    private updateValues() {
        this.values = [];

        for (let field of this.fields) {
            this.values.push(this.getValue(field));
        }
    }

    private getValue(field: FieldDto): any {
        const contentField = this.content.data[field.name];

        if (!contentField) {
            return '';
        }

        const properties = field.properties;

        let value: any;

        if (properties.isLocalizable) {
            value = contentField[this.language.iso2Code];
        } else {
            value = contentField['iv'];
        }

        if (value) {
            if (properties.fieldType === 'Json') {
                value = 'Json';
            } else if (properties.fieldType === 'Geolocation') {
                value = `${value.longitude}, ${value.latitude}`;
            } else if (properties.fieldType === 'Boolean') {
                value = value ? '✔' : '-';
            } else if (properties.fieldType === 'DateTime') {
                try {
                    const parsed = DateTime.parseISO_UTC(value);

                    if (properties['editor'] === 'Date') {
                        value = parsed.toStringFormat('YYYY-MM-DD');
                    } else {
                        value = parsed.toStringFormat('YYYY-MM-DD hh:mm:ss');
                    }
                } catch (ex) {
                    value = value;
                }
            }
        }

        return value;
    }
}

