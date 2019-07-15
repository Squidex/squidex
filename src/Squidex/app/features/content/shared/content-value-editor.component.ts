/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-content-value-editor',
    template: `
        <div [formGroup]="form">
            <ng-container [ngSwitch]="field.properties.fieldType">
                <ng-container *ngSwitchCase="'Number'">
                    <ng-container [ngSwitch]="field.properties['editor']">
                        <ng-container *ngSwitchCase="'Input'">
                            <input class="form-control" type="number" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" />
                        </ng-container>
                        <ng-container *ngSwitchCase="'Dropdown'">
                            <select class="form-control" [formControlName]="field.name">
                                <option [ngValue]="null"></option>
                                <option *ngFor="let value of field.properties['allowedValues']" [ngValue]="value">{{value}}</option>
                            </select>
                        </ng-container>
                    </ng-container>
                </ng-container>
                <ng-container *ngSwitchCase="'String'">
                    <ng-container [ngSwitch]="field.properties['editor']">
                        <ng-container *ngSwitchCase="'Input'">
                            <input class="form-control" type="text" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" />
                        </ng-container>
                        <ng-container *ngSwitchCase="'Slug'">
                            <input class="form-control" type="text" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" sqxTransformInput="Slugify" />
                        </ng-container>
                        <ng-container *ngSwitchCase="'Dropdown'">
                            <select class="form-control" [formControlName]="field.name">
                                <option [ngValue]="null"></option>
                                <option *ngFor="let value of field.properties['allowedValues']" [ngValue]="value">{{value}}</option>
                            </select>
                        </ng-container>
                    </ng-container>
                </ng-container>
                <ng-container *ngSwitchCase="'Boolean'">
                    <ng-container [ngSwitch]="field.properties['editor']">
                        <ng-container *ngSwitchCase="'Toggle'">
                            <sqx-toggle [formControlName]="field.name" [threeStates]="!field.properties.isRequired"></sqx-toggle>
                        </ng-container>
                        <ng-container *ngSwitchCase="'Checkbox'">
                            <ng-container class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" [formControlName]="field.name" sqxIndeterminateValue />
                            </ng-container>
                        </ng-container>
                    </ng-container>
                </ng-container>
            </ng-container>
        </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentValueEditorComponent {
    @Input()
    public field: FieldDto;

    @Input()
    public form: FormGroup;
}