/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* tslint:disable:component-selector */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    AppLanguageDto,
    ContentDto,
    getContentValue
} from '@app/shared';

@Component({
    selector: '[sqxReferenceItem]',
    styleUrls: ['./reference-item.component.scss'],
    templateUrl: './reference-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferenceItemComponent implements OnChanges {
    @Output()
    public delete = new EventEmitter();

    @Input()
    public language: AppLanguageDto;

    @Input()
    public isCompact = false;

    @Input()
    public columnCount = 0;

    @Input('sqxReferenceItem')
    public content: ContentDto;

    public values: ReadonlyArray<any> = [];

    public ngOnChanges(changes: SimpleChanges) {
        this.updateValues();
    }

    private updateValues() {
        const values = [];

        for (let i = 0; i < this.columnCount; i++) {
            const field = this.content.referenceFields[i];

            if (field) {
                const { formatted } = getContentValue(this.content, this.language, field);

                values.push(formatted);
            } else {
                values.push('');
            }
        }

        this.values = values;
    }
}