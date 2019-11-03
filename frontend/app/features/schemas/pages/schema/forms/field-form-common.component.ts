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
    selector: 'sqx-field-form-common',
    template: `
        <div [formGroup]="editForm">
            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldName">Name</label>

                <div class="col-7">
                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldName" readonly [ngModel]="field.name" [ngModelOptions]="standalone" />

                    <sqx-form-hint>
                        The name of the field in the API response.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldLabel">Label</label>

                <div class="col-7">
                    <sqx-control-errors for="label" [submitted]="editFormSubmitted"></sqx-control-errors>

                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldLabel" maxlength="100" formControlName="label" />

                    <sqx-form-hint>
                        Display name for documentation and user interfaces.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldHints">Hints</label>

                <div class="col-7">
                    <sqx-control-errors for="hints" [submitted]="editFormSubmitted"></sqx-control-errors>

                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldHints" maxlength="100" formControlName="hints" />

                    <sqx-form-hint>
                        Describe this field for documentation and user interfaces.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row" *ngIf="field.properties.isContentField">
                 <label class="col-3 col-form-label">Tags</label>

                <div class="col-7">
                    <sqx-tag-editor id="schemaTags" formControlName="tags"></sqx-tag-editor>

                    <sqx-form-hint>
                        Tags to annotate your field for automation processes.
                    </sqx-form-hint>
                </div>
            </div>
        </div>
    `
})
export class FieldFormCommonComponent {
    public readonly standalone = { standalone: true };

    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted = false;

    @Input()
    public field: FieldDto;
}