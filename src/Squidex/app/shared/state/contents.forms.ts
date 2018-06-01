/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


// tslint:disable:prefer-for-of

import { FormArray, FormControl, FormGroup } from '@angular/forms';

import {
    ErrorDto,
    Form,
    ImmutableArray,
    Types
} from '@app/framework';

import { AppLanguageDto } from './../services/app-languages.service';
import { fieldInvariant, RootFieldDto, SchemaDetailsDto } from './../services/schemas.service';

export class EditContentForm extends Form<FormGroup> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly languages: ImmutableArray<AppLanguageDto>
    ) {
        super(new FormGroup({}));

        for (const field of schema.fields) {
            const fieldForm = new FormGroup({});
            const fieldDefault = field.defaultValue();

            const createControl = (isOptional: boolean) => {
                if (field.properties.fieldType === 'Array') {
                    return new FormArray([], field.createValidators(isOptional));
                } else {
                    return new FormControl(fieldDefault, field.createValidators(isOptional));
                }
            };

            if (field.isLocalizable) {
                for (let language of this.languages.values) {
                    fieldForm.setControl(language.iso2Code, createControl(language.isOptional));
                }
            } else {
                fieldForm.setControl(fieldInvariant, createControl(false));
            }

            this.form.setControl(field.name, fieldForm);
        }

        this.enableContentForm();
    }

    public removeArrayItem(field: RootFieldDto, language: AppLanguageDto, index: number) {
        this.findArrayItemForm(field, language).removeAt(index);
    }

    public insertArrayItem(field: RootFieldDto, language: AppLanguageDto) {
        if (field.nested.length > 0) {
            const formControl = this.findArrayItemForm(field, language);

            this.addArrayItem(field, language, formControl);
        }
    }

    private addArrayItem(field: RootFieldDto, language: AppLanguageDto | null, formControl: FormArray) {
        const formItem = new FormGroup({});

        let isOptional = field.isLocalizable && language !== null && language.isOptional;

        for (let nested of field.nested) {
            const nestedDefault = field.defaultValue();

            formItem.setControl(nested.name, new FormControl(nestedDefault, nested.createValidators(isOptional)));
        }

        formControl.push(formItem);
    }

    private findArrayItemForm(field: RootFieldDto, language: AppLanguageDto): FormArray {
        const fieldForm = this.form.get(field.name)!;

        if (field.isLocalizable) {
            return <FormArray>fieldForm.get(language.iso2Code)!;
        } else {
            return <FormArray>fieldForm.get(fieldInvariant);
        }
    }

    public submitCompleted(newValue?: any) {
        super.submitCompleted(newValue);

        this.enableContentForm();
    }

    public submitFailed(error?: string | ErrorDto) {
        super.submitFailed(error);

        this.enableContentForm();
    }

    public loadData(value: any, isArchive: boolean) {
        for (let field of this.schema.fields) {
            if (field.properties.fieldType === 'Array' && field.nested.length > 0) {
                const fieldValue = value ? value[field.name] || {} : {};
                const fieldForm = <FormGroup>this.form.get(field.name)!;

                const addControls = (key: string, language: AppLanguageDto | null) => {
                    const languageValue = fieldValue[key];
                    const languageForm = new FormArray([]);

                    if (Types.isArray(languageValue)) {
                        for (let i = 0; i < languageValue.length; i++) {
                            this.addArrayItem(field, language, languageForm);
                        }
                    }

                    fieldForm.setControl(key, languageForm);
                };

                if (field.isLocalizable) {
                    for (let language of this.languages.values) {
                        addControls(language.iso2Code, language);
                    }
                } else {
                    addControls(fieldInvariant, null);
                }
            }
        }

        super.load(value);

        if (isArchive) {
            this.form.disable();
        } else {
            this.enableContentForm();
        }
    }

    private enableContentForm() {
        if (this.schema.fields.length === 0) {
            this.form.enable();
        } else {
            for (const field of this.schema.fields) {
                const fieldForm = this.form.controls[field.name];

                if (field.isDisabled) {
                    fieldForm.disable();
                } else {
                    fieldForm.enable();
                }
            }
        }
    }
}

export class PatchContentForm extends Form<FormGroup> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        for (let field of this.schema.listFields) {
            if (field.properties && field.properties['inlineEditable']) {
                this.form.setControl(field.name, new FormControl(undefined, field.createValidators(this.language.isOptional)));
            }
        }
    }

    public submit() {
        const result = super.submit();

        if (result) {
            const request = {};

            for (let field of this.schema.listFields) {
                if (field.properties['inlineEditable']) {
                    const value = result[field.name];

                    if (field.isLocalizable) {
                        request[field.name] = { [this.language.iso2Code]: value };
                    } else {
                        request[field.name] = { iv: value };
                    }
                }
            }

            return request;
        }

        return result;
    }
}