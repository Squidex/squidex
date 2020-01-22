/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto, PatternDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-validation',
    template: `
        <ng-container [ngSwitch]="field.rawProperties.fieldType">
            <ng-container *ngSwitchCase="'Array'">
                <sqx-array-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-array-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Assets'">
                <sqx-assets-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-assets-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'DateTime'">
                <sqx-date-time-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-date-time-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Boolean'">
                <sqx-boolean-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-boolean-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Geolocation'">
                <sqx-geolocation-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-geolocation-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Json'">
                <sqx-json-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-json-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Number'">
                <sqx-number-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-number-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'References'">
                <sqx-references-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-references-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'String'">
                <sqx-string-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties" [patterns]="patterns"></sqx-string-validation>
            </ng-container>
            <ng-container *ngSwitchCase="'Tags'">
                <sqx-tags-validation [editForm]="editForm" [field]="field" [properties]="field.rawProperties"></sqx-tags-validation>
            </ng-container>
        </ng-container>
    `
})
export class FieldFormValidationComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public patterns: ReadonlyArray<PatternDto>;
}