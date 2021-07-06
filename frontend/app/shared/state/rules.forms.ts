/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx } from '@app/framework';
import { RuleElementDto } from '../services/rules.service';

export class ActionForm extends Form<any, FormGroup> {
    constructor(public readonly definition: RuleElementDto,
        private readonly actionType: string,
    ) {
        super(ActionForm.builForm(definition));
    }

    private static builForm(definition: RuleElementDto) {
        const controls: { [name: string]: AbstractControl } = {};

        for (const property of definition.properties) {
            const validator =
                property.isRequired ?
                Validators.required :
                Validators.nullValidator;

            controls[property.name] = new FormControl(undefined, validator);
        }

        return new FormGroup(controls);
    }

    protected transformSubmit(value: any): any {
        value.actionType = this.actionType;

        return value;
    }
}

export class TriggerForm extends Form<any, FormGroup> {
    constructor(formBuilder: FormBuilder,
        private readonly triggerType: string,
    ) {
        super(TriggerForm.builForm(formBuilder, triggerType));
    }

    private static builForm(formBuilder: FormBuilder, triggerType: string) {
        switch (triggerType) {
            case 'ContentChanged': {
                return formBuilder.group({ handleAll: false, schemas: undefined });
            }
            case 'Usage': {
                return formBuilder.group({
                    limit: [20000,
                        [
                            Validators.required,
                        ],
                    ],
                    numDays: [3,
                        [
                            ValidatorsEx.between(1, 30),
                        ],
                    ],
                });
            }
            default: {
                return formBuilder.group({ condition: undefined });
            }
        }
    }

    protected transformSubmit(value: any): any {
        value.triggerType = this.triggerType;

        return value;
    }
}
