/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, UndefinableFormGroup, ValidatorsEx } from '@app/framework';
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
    constructor(
        private readonly triggerType: string,
    ) {
        super(TriggerForm.builForm(triggerType));
    }

    private static builForm(triggerType: string) {
        switch (triggerType) {
            case 'ContentChanged': {
                return new UndefinableFormGroup({
                    handleAll: new FormControl(false,
                        Validators.nullValidator,
                    ),
                    schemas: new FormControl(undefined,
                        Validators.nullValidator,
                    ),
                });
            }
            case 'Usage': {
                return new UndefinableFormGroup({
                    limit: new FormControl(20000,
                        Validators.required,
                    ),
                    numDays: new FormControl(3,
                        ValidatorsEx.between(1, 30),
                    ),
                });
            }
            default: {
                return new UndefinableFormGroup({
                    condition: new FormControl('',
                        Validators.nullValidator,
                    ),
                });
            }
        }
    }

    protected transformSubmit(value: any): any {
        value.triggerType = this.triggerType;

        return value;
    }
}
