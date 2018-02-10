/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';

import {
    AppContext,
    ContentDto,
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
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentItemComponent implements OnInit, OnChanges {
    @Output()
    public publishing = new EventEmitter();

    @Output()
    public unpublishing = new EventEmitter();

    @Output()
    public archiving = new EventEmitter();

    @Output()
    public restoring = new EventEmitter();

    @Output()
    public deleting = new EventEmitter();

    @Output()
    public selectedChange = new EventEmitter();

    @Input()
    public selected = false;

    @Input()
    public columnWidth: number;

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

    constructor(public readonly ctx: AppContext
    ) {
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

