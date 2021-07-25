/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* tslint:disable: component-selector */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AppLanguageDto, ContentDto, getContentValue } from '@app/shared';

@Component({
    selector: '[sqxReferenceItem][language]',
    styleUrls: ['./reference-item.component.scss'],
    templateUrl: './reference-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferenceItemComponent implements OnChanges {
    @Output()
    public delete = new EventEmitter();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public language: AppLanguageDto;

    @Input()
    public canRemove?: boolean | null = true;

    @Input()
    public isCompact?: boolean | null;

    @Input()
    public isDisabled?: boolean | null;

    @Input()
    public validations: { [id: string]: boolean };

    @Input()
    public validityVisible?: boolean | null;

    @Input()
    public columns = 0;

    @Input('sqxReferenceItem')
    public content: ContentDto;

    public get valid() {
        return !this.validations ? undefined : this.validations[this.content.id];
    }

    public values: ReadonlyArray<any> = [];

    public ngOnChanges() {
        const values = [];

        for (let i = 0; i < this.columns; i++) {
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
