/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    LanguageDto,
    MetaFields,
    Query,
    RootFieldDto,
    TableField,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-content-list-header',
    template: `
        <ng-container [ngSwitch]="fieldName">
            <ng-container *ngSwitchCase="metaFields.id">
                <sqx-table-header text="Id"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.created">
                <sqx-table-header text="Created"
                    [sortable]="true"
                    [fieldPath]="'created'"
                    [query]="query"
                    (queryChange)="queryChange.emit($event)"
                    [language]="language">
                </sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.createdByAvatar">
                <sqx-table-header text="By"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.createdByName">
                <sqx-table-header text="Created By"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModified">
                <sqx-table-header text="Updated"
                    [sortable]="true"
                    [fieldPath]="'lastModified'"
                    [query]="query"
                    (queryChange)="queryChange.emit($event)"
                    [language]="language">
                </sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModifiedByAvatar">
                <sqx-table-header text="By"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.lastModifiedByName">
                <sqx-table-header text="Modified By"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.status">
                <sqx-table-header text="Status"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.statusNext">
                <sqx-table-header text="Next Status"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.statusColor">
                <sqx-table-header text="Status"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchCase="metaFields.version">
                <sqx-table-header text="Version"></sqx-table-header>
            </ng-container>
            <ng-container *ngSwitchDefault>
                <sqx-table-header [text]="fieldDisplayName"
                    [sortable]="isSortable"
                    [fieldPath]="fieldPath"
                    [query]="query"
                    (queryChange)="queryChange.emit($event)"
                    [language]="language">
                </sqx-table-header>
            </ng-container>
        </ng-container>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListHeaderComponent {
    @Input()
    public field: TableField;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query;

    @Input()
    public language: LanguageDto;

    public get metaFields() {
        return MetaFields;
    }

    public get isSortable() {
        return Types.is(this.field, RootFieldDto) ? this.field.properties.isSortable : false;
    }

    public get fieldName() {
        return Types.is(this.field, RootFieldDto) ? this.field.name : this.field;
    }

    public get fieldDisplayName() {
        return Types.is(this.field, RootFieldDto) ? this.field.displayName : '';
    }

    public get fieldPath() {
        if (Types.isString(this.field)) {
            return this.field;
        } else if (this.field.isLocalizable && this.language) {
            return `data.${this.field.name}.${this.language.iso2Code}`;
        } else {
            return `data.${this.field.name}.iv`;
        }
    }
}