/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, ValidatorsEx } from '@app/framework';
import { RuleElementDto } from '../services/rules.service';

export class ActionForm extends Form<any, UntypedFormGroup> {
    constructor(public readonly definition: RuleElementDto,
        public readonly actionType: string,
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

            let defaultValue = undefined;

            if (property.isRequired && property.options) {
                defaultValue = property.options[0];
            }

            controls[property.name] = new UntypedFormControl(defaultValue, validator);
        }

        return new ExtendedFormGroup(controls);
    }

    protected transformSubmit(value: any): any {
        value.actionType = this.actionType;

        return value;
    }
}

export class TriggerForm extends Form<any, UntypedFormGroup> {
    constructor(
        public readonly triggerType: string,
    ) {
        super(TriggerForm.builForm(triggerType));
    }

    private static builForm(triggerType: string) {
        switch (triggerType) {
            case 'ContentChanged': {
                return new ExtendedFormGroup({
                    handleAll: new UntypedFormControl(false,
                        Validators.nullValidator,
                    ),
                    schemas: new UntypedFormControl(undefined,
                        Validators.nullValidator,
                    ),
                });
            }
            case 'Usage': {
                return new ExtendedFormGroup({
                    limit: new UntypedFormControl(20000,
                        Validators.required,
                    ),
                    numDays: new UntypedFormControl(3,
                        ValidatorsEx.between(1, 30),
                    ),
                });
            }
            default: {
                return new ExtendedFormGroup({
                    condition: new UntypedFormControl('',
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
