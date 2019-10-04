/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    AppLanguageDto,
    ContentDto,
    getContentValue,
    RootFieldDto
} from '@app/shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContentSelectorItem]',
    template: `
        <tr (click)="toggle()">
            <td class="cell-select">
                <input type="checkbox" class="form-check"
                    [disabled]="!selectable"
                    [ngModel]="selected || !selectable"
                    (ngModelChange)="emitSelectedChange($event)" />
            </td>

            <td class="cell-user">
                <img class="user-picture" title="{{content.lastModifiedBy | sqxUserNameRef}}" [attr.src]="content.lastModifiedBy | sqxUserPictureRef" />
            </td>

            <td class="cell-auto cell-content" *ngFor="let value of values">
                <sqx-content-value [value]="value"></sqx-content-value>
            </td>

            <td class="cell-time">
                <sqx-content-status
                    [status]="content.status"
                    [statusColor]="content.statusColor"
                    [scheduledTo]="content.scheduleJob?.status"
                    [scheduledAt]="content.scheduleJob?.dueTime"
                    [isPending]="content.isPending">
                </sqx-content-status>

                <small class="item-modified">{{content.lastModified | sqxFromNow}}</small>
            </td>
        </tr>
        <tr class="spacer"></tr>
        `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentSelectorItemComponent implements OnChanges {
    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input()
    public selected = false;

    @Input()
    public selectable = true;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public fields: RootFieldDto[];

    @Input('sqxContentSelectorItem')
    public content: ContentDto;

    public values: any[] = [];

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content'] || changes['language']) {
            this.updateValues();
        }
    }

    public toggle() {
        if (this.selectable) {
            this.emitSelectedChange(!this.selected);
        }
    }

    public emitSelectedChange(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }

    private updateValues() {
        this.values = [];

        for (let field of this.fields) {
            const { formatted } = getContentValue(this.content, this.language, field);

            this.values.push(formatted);
        }
    }
}