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
    getContentValue
} from '@app/shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxReferenceItem]',
    styleUrls: ['./reference-item.component.scss'],
    template: `
        <tr>
            <td class="cell-select">
                <ng-content></ng-content>
            </td>

            <td class="cell-user" *ngIf="!isCompact">
                <img class="user-picture" title="{{content.lastModifiedBy | sqxUserNameRef}}" [attr.src]="content.lastModifiedBy | sqxUserPictureRef" />
            </td>

            <td class="cell-auto cell-content" *ngFor="let value of values">
                <sqx-content-value [value]="value"></sqx-content-value>
            </td>

            <td class="cell-label" *ngIf="!isCompact">
                <span class="badge badge-pill truncate-inline badge-primary">{{content.schemaDisplayName}}</span>
            </td>

            <td class="cell-actions">
                <div class="reference-edit">
                    <button type="button" class="btn btn-text-secondary">
                        <i class="icon-dots"></i>
                    </button>

                    <div class="reference-menu">
                        <a class="btn btn-text-secondary" [routerLink]="['../..', content.schemaName, content.id]">
                            <i class="icon-pencil"></i>
                        </a>

                        <button type="button" class="btn btn-text-secondary" (click)="emitDelete()">
                            <i class="icon-close"></i>
                        </button>
                    </div>
                </div>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
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

    public emitDelete() {
        this.delete.emit();
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