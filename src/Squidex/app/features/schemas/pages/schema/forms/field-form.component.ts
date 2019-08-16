/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import {
    FieldDto,
    ImmutableArray,
    PatternDto
} from '@app/shared';

@Component({
    selector: 'sqx-field-form',
    template: `
        <div class="table-items-row-details-tabs clearfix">
            <ul class="nav nav-tabs2">
                <li class="nav-item">
                    <a class="nav-link" (click)="selectTab(0)" [class.active]="selectedTab === 0">Common</a>
                </li>
                <li class="nav-item" [class.hidden]="!field.properties.isContentField">
                    <a class="nav-link" (click)="selectTab(1)" [class.active]="selectedTab === 1">Validation</a>
                </li>
                <li class="nav-item" [class.hidden]="!field.properties.isContentField || field.properties.fieldType === 'Assets'">
                    <a class="nav-link" (click)="selectTab(2)" [class.active]="selectedTab === 2">Editing</a>
                </li>
            </ul>

            <div class="float-right" *ngIf="showButtons">
                <button [disabled]="field.isLocked" type="reset" class="btn btn-secondary" (click)="cancel.emit()">Cancel</button>
                <button [disabled]="field.isLocked" type="submit" class="btn btn-primary ml-1" *ngIf="isEditable">Save</button>
            </div>
        </div>

        <div class="table-items-row-details-tab" [class.hidden]="selectedTab !== 0">
            <sqx-field-form-common [editForm]="editForm" [editFormSubmitted]="editFormSubmitted " [field]="field"></sqx-field-form-common>
        </div>

        <div class="table-items-row-details-tab" [class.hidden]="selectedTab !== 1">
            <sqx-field-form-validation [patterns]="patterns" [editForm]="editForm" [field]="field"></sqx-field-form-validation>
        </div>

        <div class="table-items-row-details-tab" [class.hidden]="selectedTab !== 2">
            <sqx-field-form-ui [editForm]="editForm" [field]="field"></sqx-field-form-ui>
        </div>
    `
})
export class FieldFormComponent implements AfterViewInit {
    @Input()
    public showButtons: boolean;

    @Input()
    public isEditable: boolean;

    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted: boolean;

    @Input()
    public patterns: ImmutableArray<PatternDto>;

    @Input()
    public field: FieldDto;

    @Output()
    public cancel = new EventEmitter();

    public selectedTab = 0;

    public ngAfterViewInit() {
        if (!this.isEditable) {
            this.editForm.disable();
        } else {
            this.editForm.enable();
        }
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }
}