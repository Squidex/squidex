/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';

import {
    ContentDto,
    getContentValue,
    LanguageDto,
    MetaFields,
    RootFieldDto,
    TableField,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-content-list-field',
    template: `
        <ng-container [ngSwitch]="fieldName">
            <ng-container *ngSwitchCase="metaFields.id">
                <small class="truncate">{{content.id}}</small>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.created">
                <small class="truncate">{{content.created | sqxFromNow}}</small>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.createdByAvatar">
                <img class="user-picture" title="{{content.createdBy | sqxUserNameRef}}" [src]="content.createdBy | sqxUserPictureRef" />
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.createdByName">
                <small class="truncate">{{content.createdBy | sqxUserNameRef}}</small>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModified">
                <small class="truncate">{{content.lastModified | sqxFromNow}}</small>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModifiedByAvatar">
                <img class="user-picture" title="{{content.lastModifiedBy | sqxUserNameRef}}" [src]="content.lastModifiedBy | sqxUserPictureRef" />
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModifiedByName">
                <small class="truncate">{{content.lastModifiedBy | sqxUserNameRef}}</small>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.status">
                <span class="truncate">
                    <sqx-content-status
                        [status]="content.status"
                        [statusColor]="content.statusColor"
                        [scheduledTo]="content.scheduleJob?.status"
                        [scheduledAt]="content.scheduleJob?.dueTime"
                        [isPending]="content.isPending">
                    </sqx-content-status>

                    {{content.status}}
                </span>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.statusNext">
                <span class="truncate" *ngIf="content.scheduleJob; let job">
                    {{job.status}} at {{job.dueTime | sqxShortDate}}
                </span>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.statusColor">
                <sqx-content-status
                    [status]="content.status"
                    [statusColor]="content.statusColor"
                    [scheduledTo]="content.scheduleJob?.status"
                    [scheduledAt]="content.scheduleJob?.dueTime"
                    [isPending]="content.isPending">
                </sqx-content-status>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.version">
                <small class="truncate">{{content.version.value}}</small>
            </ng-container>
            <ng-container *ngSwitchDefault>
                <ng-container *ngIf="isInlineEditable && patchAllowed; else displayTemplate">
                    <sqx-content-value-editor [form]="patchForm" [field]="field"></sqx-content-value-editor>
                </ng-container>

                <ng-template #displayTemplate>
                    <sqx-content-value [value]="value"></sqx-content-value>
                </ng-template>
            </ng-container>
        </ng-container>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListFieldComponent implements OnChanges {
    @Input()
    public field: TableField;

    @Input()
    public content: ContentDto;

    @Input()
    public patchAllowed: boolean;

    @Input()
    public patchForm: FormGroup;

    @Input()
    public language: LanguageDto;

    public value: any;

    public ngOnChanges() {
        this.reset();
    }

    public reset() {
        if (Types.is(this.field, RootFieldDto)) {
            const { value, formatted } = getContentValue(this.content, this.language, this.field);

            if (this.patchForm) {
                const formControl = this.patchForm.controls[this.field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }

            this.value = formatted;
        }
    }

    public get metaFields() {
        return MetaFields;
    }

    public get isInlineEditable() {
        return Types.is(this.field, RootFieldDto) ? this.field.isInlineEditable : false;
    }

    public get fieldName() {
        return Types.is(this.field, RootFieldDto) ? this.field.name : this.field;
    }
}