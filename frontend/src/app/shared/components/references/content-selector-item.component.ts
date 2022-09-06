/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* tslint:disable: component-selector */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ContentDto, LanguageDto, META_FIELDS, SchemaDto } from '@app/shared/internal';

@Component({
    selector: '[sqxContentSelectorItem][language][languages][schema]',
    styleUrls: ['./content-selector-item.component.scss'],
    templateUrl: './content-selector-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentSelectorItemComponent {
    public readonly metaFields = META_FIELDS;

    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input()
    public selected?: boolean | null;

    @Input()
    public selectable?: boolean | null = true;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public schema!: SchemaDto;

    @Input('sqxContentSelectorItem')
    public content!: ContentDto;

    public toggle() {
        if (this.selectable) {
            this.select(!this.selected);
        }
    }

    public select(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }
}
