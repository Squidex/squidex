/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    ContentDto,
    DialogService,
    fadeAnimation,
    FieldDto,
    ModalView,
    SchemaDto
} from 'shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content-item.component.scss'],
    templateUrl: './content-item.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentItemComponent extends AppComponentBase implements OnInit, OnChanges {
    @Output()
    public publishing = new EventEmitter<ContentDto>();

    @Output()
    public unpublishing = new EventEmitter<ContentDto>();

    @Output()
    public deleting = new EventEmitter<ContentDto>();

    @Input()
    public languageCode: string;

    @Input()
    public schemaFields: FieldDto[];

    @Input()
    public schema: SchemaDto;

    @Input()
    public isReadOnly = false;

    @Input()
    public isReference = false;

    @Input('sqxContent')
    public content: ContentDto;

    public dropdown = new ModalView(false, true);

    public values: any[] = [];

    constructor(apps: AppsStoreService, dialogs: DialogService) {
        super(dialogs, apps);
    }

    public ngOnChanges() {
        this.updateValues();
    }

    public ngOnInit() {
        this.updateValues();
    }

    private updateValues() {
        this.values = [];

        if (this.schemaFields) {
            for (let field of this.schemaFields) {
                this.values.push(this.getValue(field));
            }
        }
    }

    private getValue(field: FieldDto): any {
        const contentField = this.content.data[field.name];

        if (contentField) {
            if (field.partitioning === 'language') {
                return field.formatValue(contentField[this.languageCode]);
            } else {
                return field.formatValue(contentField['iv']);
            }
        } else {
            return '';
        }
    }
}

