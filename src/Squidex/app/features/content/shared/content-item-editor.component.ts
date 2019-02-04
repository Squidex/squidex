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
    selector: 'sqx-content-item-editor',
    template: `
        <div [formGroup]="form">
            <div [ngSwitch]="field.properties.fieldType">
                <div *ngSwitchCase="'Number'">
                    <div [ngSwitch]="field.properties['editor']">
                        <div *ngSwitchCase="'Input'">
                            <input class="form-control" type="number" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" />
                        </div>
                        <div *ngSwitchCase="'Dropdown'">
                            <select class="form-control" [formControlName]="field.name">
                                <option [ngValue]="null"></option>
                                <option *ngFor="let value of field.properties['allowedValues']" [ngValue]="value">{{value}}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div *ngSwitchCase="'String'">
                    <div [ngSwitch]="field.properties['editor']">
                        <div *ngSwitchCase="'Input'">
                            <input class="form-control" type="text" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" />
                        </div>
                        <div *ngSwitchCase="'Slug'">
                            <input class="form-control" type="text" [formControlName]="field.name" [placeholder]="field.displayPlaceholder" sqxTransformInput="Slugify" />
                        </div>
                        <div *ngSwitchCase="'Dropdown'">
                            <select class="form-control" [formControlName]="field.name">
                                <option [ngValue]="null"></option>
                                <option *ngFor="let value of field.properties['allowedValues']" [ngValue]="value">{{value}}</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div *ngSwitchCase="'Boolean'">
                    <div [ngSwitch]="field.properties['editor']">
                        <div *ngSwitchCase="'Toggle'">
                            <sqx-toggle [formControlName]="field.name" [threeStates]="!field.properties.isRequired"></sqx-toggle>
                        </div>
                        <div *ngSwitchCase="'Checkbox'">
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" [formControlName]="field.name" sqxIndeterminateValue />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentItemEditorComponent {
    @Input()
    public field: FieldDto;

    @Input()
    public form: FormGroup;
}