/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ResourceOwner } from '@app/framework';
import { FieldWrappings, HtmlValue, RootFieldDto, TableField, TableSettings, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value[value]',
    styleUrls: ['./content-value.component.scss'],
    templateUrl: './content-value.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentValueComponent extends ResourceOwner implements OnChanges {
    @Input()
    public value!: any;

    @Input()
    public field!: TableField;

    @Input()
    public fields?: TableSettings;

    public wrapping = false;

    public get isString() {
        return Types.is(this.field, RootFieldDto) && this.field.properties.fieldType === 'String';
    }

    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
    ) {
        super();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fields']) {
            this.unsubscribeAll();

            this.own(this.fields?.fieldWrappings
                .subscribe(wrappings => {
                    this.updateWrapping(wrappings);
                }));
        }
    }

    public toggle() {
        this.fields?.toggleWrapping(this.field?.['name']);
    }

    private updateWrapping(wrappings: FieldWrappings) {
        const wrapping = wrappings[this.field?.['name']];

        if (wrapping === this.wrapping) {
            return;
        }

        this.wrapping = wrapping;

        this.changeDetector.detectChanges();
    }
}
