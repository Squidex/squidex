/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* tslint:disable:component-selector */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    ContentDto,
    LanguageDto,
    SchemaDetailsDto
} from '@app/shared';

@Component({
    selector: '[sqxContentSelectorItem]',
    styleUrls: ['./content-selector-item.component.scss'],
    templateUrl: './content-selector-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentSelectorItemComponent {
    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input()
    public selected = false;

    @Input()
    public selectable = true;

    @Input()
    public language: LanguageDto;

    @Input()
    public schema: SchemaDetailsDto;

    @Input('sqxContentSelectorItem')
    public content: ContentDto;

    public toggle() {
        if (this.selectable) {
            this.select(!this.selected);
        }
    }

    public select(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }
}