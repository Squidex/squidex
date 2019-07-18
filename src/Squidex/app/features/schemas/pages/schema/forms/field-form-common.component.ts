/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common',
    template: `
        <div [formGroup]="editForm">
            <div class="form-group row">
                <label class="col-3 col-form-label" for="{{field.fieldId}}_fieldName">Name</label>

                <div class="col-6">
                    <input type="text" class="form-control" id="{{field.fieldId}}_fieldName" readonly [ngModel]="field.name" [ngModelOptions]="{standalone: true}" />

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
                <div class="col-6 offset-3">
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
                <div class="col-6 offset-3">
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
        </div>
    `
})
export class FieldFormCommonComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted = false;

    @Input()
    public field: FieldDto;

    public ngOnInit() {
        this.editForm.setControl('isRequired',
            new FormControl(this.field.properties.isRequired));

        this.editForm.setControl('isListField',
            new FormControl(this.field.properties.isListField));

        this.editForm.setControl('isReferenceField',
            new FormControl(this.field.properties.isReferenceField));

        this.editForm.setControl('editorUrl',
            new FormControl(this.field.properties.editorUrl));

        this.editForm.setControl('hints',
            new FormControl(this.field.properties.hints));

        this.editForm.setControl('placeholder',
            new FormControl(this.field.properties.placeholder));

        this.editForm.setControl('label',
            new FormControl(this.field.properties.label));
    }
}