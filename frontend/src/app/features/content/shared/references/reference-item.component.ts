/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output } from '@angular/core';
import { ContentDto, getContentValue, LanguageDto, META_FIELDS } from '@app/shared';

@Component({
    selector: '[sqxReferenceItem][language][languages]',
    styleUrls: ['./reference-item.component.scss'],
    templateUrl: './reference-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferenceItemComponent {
    public readonly metaFields = META_FIELDS;

    @Output()
    public delete = new EventEmitter();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public canRemove?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public isCompact?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isDisabled?: boolean | null;

    @Input({ transform: booleanAttribute })
    public validations?: { [id: string]: boolean };

    @Input({ transform: booleanAttribute })
    public validityVisible?: boolean | null;

    @Input({ transform: numberAttribute })
    public columns = 0;

    @Input('sqxReferenceItem')
    public content!: ContentDto;

    public values: ReadonlyArray<any> = [];

    public get isValid() {
        return !this.validations ? undefined : this.validations[this.content.id];
    }

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
