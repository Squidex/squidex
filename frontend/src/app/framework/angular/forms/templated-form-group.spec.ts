/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { FormGroupTemplate, TemplatedFormGroup } from './templated-form-group';

describe('TemplatedFormGroup', () => {
    class Template implements FormGroupTemplate {
        public clearCalled = 0;
        public removeCalled: number[] = [];

        public setControls(form: UntypedFormGroup) {
            form.setControl('value', new UntypedFormControl());
        }

        public clearControls() {
            this.clearCalled++;
        }
    }

    let formTemplate: Template;
    let formArray: TemplatedFormGroup;

    beforeEach(() => {
        formTemplate = new Template();
        formArray = new TemplatedFormGroup(formTemplate);
    });

    type Test = [ (value: any) => void, string];

    const methods: Test[] = [
        [x => formArray.setValue(x), 'setValue'],
        [x => formArray.patchValue(x), 'patchValue'],
        [x => formArray.reset(x), 'reset'],
    ];

    methods.forEach(([method, name]) => {
        it(`Should call template to construct controls for ${name}`, () => {
            const value1 = {
                value: 1,
            };

            method(value1);

            expect(formArray.value).toEqual(value1);
        });
        it(`Should call template to clear items with for ${name}`, () => {
            const value1 = {
                value: 1,
            };

            method(value1);
            method(undefined);

            expect(formArray.value).toEqual(undefined);
            expect(formTemplate.clearCalled).toEqual(1);
            expect(formTemplate.removeCalled).toEqual([]);
        });
    });
});
