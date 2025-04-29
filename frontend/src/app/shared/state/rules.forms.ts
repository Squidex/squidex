/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, TemplatedFormArray, ValidatorsEx } from '@app/framework';
import { RuleElementDto, RuleTriggerDto } from '../model';

export class ActionForm extends Form<UntypedFormGroup, Record<string, any>> {
    public get editableProperties() {
        return this.definition.properties.filter(x => x.editor !== 'None');
    }

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

            if (property.editor === 'Branches') {
                controls[property.name] = new TemplatedFormArray(BranchTemplate.INSTANCE, validator);
            } else {
                controls[property.name] = new UntypedFormControl(defaultValue, validator);
            }
        }

        return new ExtendedFormGroup(controls);
    }

    public branch(name: string) {
        return this.form.controls[name] as TemplatedFormArray;
    }

    protected transformSubmit(value: any): any {
        value.actionType = this.actionType;
        return value;
    }
}

class BranchTemplate {
    public static readonly INSTANCE = new BranchTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            condition: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            step: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        });
    }
}

export class TriggerForm extends Form<UntypedFormGroup, RuleTriggerDto> {
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
                    schemas: new TemplatedFormArray(ContentChangedSchemaTemplate.INSTANCE,
                        Validators.nullValidator,
                    ),
                    referencedSchemas: new TemplatedFormArray(ContentChangedSchemaTemplate.INSTANCE,
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

    protected transformSubmit(value: any) {
        value.triggerType = this.triggerType;

        return RuleTriggerDto.fromJSON(value);
    }
}


class ContentChangedSchemaTemplate {
    public static readonly INSTANCE = new ContentChangedSchemaTemplate();

    public createControl(_: any) {
        return new ExtendedFormGroup({
            schemaId: new UntypedFormControl('',
                Validators.required,
            ),
            condition: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        });
    }
}