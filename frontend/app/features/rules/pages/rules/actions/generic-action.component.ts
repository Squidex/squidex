/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { RuleElementDto } from '@app/shared';

@Component({
    selector: 'sqx-generic-action',
    styleUrls: ['./generic-action.component.scss'],
    templateUrl: './generic-action.component.html'
})
export class GenericActionComponent implements OnInit {
    @Input()
    public definition: RuleElementDto;

    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    public ngOnInit() {
        for (const property of this.definition.properties) {
            const validators = [];

            if (property.isRequired) {
                validators.push(Validators.required);
            }

            const control = new FormControl(this.action[property.name] || '', validators);

            this.actionForm.setControl(property.name, control);
        }
    }
}