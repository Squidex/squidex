/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { RuleElementDto } from '@app/shared';

@Component({
    selector: 'sqx-generic-action',
    styleUrls: ['./generic-action.component.scss'],
    templateUrl: './generic-action.component.html'
})
export class GenericActionComponent implements OnChanges {
    @Input()
    public definition: RuleElementDto;

    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['actionForm'] || changes['definition']) {
            for (const property of this.definition.properties) {
                const validator =
                    property.isRequired ?
                    Validators.required :
                    Validators.nullValidator;

                const control = new FormControl('', validator);

                this.actionForm.setControl(property.name, control);
            }
        }

        this.actionForm.patchValue(this.action);
    }
}