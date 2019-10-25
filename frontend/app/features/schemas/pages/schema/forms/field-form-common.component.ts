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

                <div class="col-6">
                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldName" readonly [ngModel]="field.name" [ngModelOptions]="standalone" />

                    <sqx-form-hint>
                        The name of the field in the API response.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldLabel">Label</label>

                <div class="col-6">
                    <sqx-control-errors for="label" [submitted]="editFormSubmitted"></sqx-control-errors>

                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldLabel" maxlength="100" formControlName="label" />

                    <sqx-form-hint>
                        Define the display name for the field for documentation and user interfaces.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldHints">Hints</label>

                <div class="col-6">
                    <sqx-control-errors for="hints" [submitted]="editFormSubmitted"></sqx-control-errors>

                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldHints" maxlength="100" formControlName="hints" />

                    <sqx-form-hint>
                        Define some hints for the user and editor for the field for documentation and user interfaces.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row" *ngIf="field.properties.isContentField">
                <div class="col-9 offset-3">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="{{field.fieldId}}_fieldListfield" formControlName="isListField" />
                        <label class="form-check-label" for="{{field.fieldId}}_fieldListfield">
                            List Field
                        </label>
                    </div>

                    <sqx-form-hint>
                        List fields are shown as a column in the content list.<br />When no list field is defined, the first field is used.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row" *ngIf="field.properties.isContentField">
                <div class="col-9 offset-3">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="{{field.fieldId}}_fieldReferencefield" formControlName="isReferenceField" />
                        <label class="form-check-label" for="{{field.fieldId}}_fieldReferencefield">
                            Reference Field
                        </label>
                    </div>

                    <sqx-form-hint>
                        Reference fields are shown as a column in the content list when referenced by another content.<br />When no reference field is defined, the first field is used.
                    </sqx-form-hint>
                </div>
            </div>

            <div class="form-group row" *ngIf="field.properties.isContentField">
                 <label class="col-3 col-form-label">Tags</label>

                <div class="col-9">
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