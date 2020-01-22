/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-ui',
    template: `
        <ng-container [ngSwitch]="field.rawProperties.fieldType">
            <ng-container *ngSwitchCase="'Assets'">
                <sqx-assets-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-assets-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'Boolean'">
                <sqx-boolean-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-boolean-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'DateTime'">
                <sqx-date-time-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-date-time-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'Geolocation'">
                <sqx-geolocation-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-geolocation-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'Json'">
                <sqx-json-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-json-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'Number'">
                <sqx-number-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-number-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'References'">
                <sqx-references-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-references-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'String'">
                <sqx-string-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-string-ui>
            </ng-container>
            <ng-container *ngSwitchCase="'Tags'">
                <sqx-tags-ui [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-tags-ui>
            </ng-container>
        </ng-container>
    `
})
export class FieldFormUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;
}