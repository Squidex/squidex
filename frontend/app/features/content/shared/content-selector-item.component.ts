/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    ContentDto,
    LanguageDto,
    SchemaDetailsDto
} from '@app/shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContentSelectorItem]',
    template: `
        <tr (click)="toggle()">
            <td class="cell-select" sqxStopClick>
                <input type="checkbox" class="form-check"
                    [disabled]="!selectable"
                    [ngModel]="selected || !selectable"
                    (ngModelChange)="emitSelectedChange($event)" />
            </td>

            <td sqxContentListCell="meta.lastModifiedBy.avatar">
                <sqx-content-list-field field="meta.lastModifiedBy.avatar" [content]="content" [language]="language"></sqx-content-list-field>
            </td>

            <td *ngFor="let field of schema.defaultReferenceFields" [sqxContentListCell]="field">
                <sqx-content-list-field [field]="field" [content]="content" [language]="language"></sqx-content-list-field>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
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
            this.emitSelectedChange(!this.selected);
        }
    }

    public emitSelectedChange(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }
}