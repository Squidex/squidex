/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { FormArrayTemplate, TemplatedFormArray } from './templated-form-array';

describe('TemplatedFormArray', () => {
    class Template implements FormArrayTemplate {
        public clearCalled = 0;
        public removeCalled: number[] = [];

        public createControl() {
            return new UntypedFormGroup({
                value: new UntypedFormControl(),
            });
        }

        public clearControls() {
            this.clearCalled++;
        }

        public removeControl(index: number) {
            this.removeCalled.push(index);
        }
    }

    let formTemplate: Template;
    let formArray: TemplatedFormArray;

    beforeEach(() => {
        formTemplate = new Template();
        formArray = new TemplatedFormArray(formTemplate);
    });

    type Test = [ (value: any) => void, string];

    const methods: Test[] = [
        [x => formArray.setValue(x), 'setValue'],
        [x => formArray.patchValue(x), 'patchValue'],
        [x => formArray.reset(x), 'reset'],
    ];

    methods.forEach(([method, name]) => {
        it(`Should call template to construct items for ${name}`, () => {
            const value1 = [{
                value: 1,
            }, {
                value: 2,
            }];

            method(value1);

            expect(formArray.value).toEqual(value1);
        });

        it(`Should call template to remove items for ${name}`, () => {
            const value1 = [{
                value: 1,
            }, {
                value: 2,
            }, {
                value: 3,
            }, {
                value: 4,
            }];

            const value2 = [{
                value: 1,
            }, {
                value: 2,
            }];

            method(value1);
            method(value2);

            expect(formArray.value).toEqual(value2);
            expect(formTemplate.clearCalled).toEqual(0);
            expect(formTemplate.removeCalled).toEqual([3, 2]);
        });

        it(`Should call template to clear items with undefined for ${name}`, () => {
            const value1 = [{
                value: 1,
            }, {
                value: 2,
            }];

            method(value1);
            method(undefined);

            expect(formArray.value).toEqual(undefined);
            expect(formTemplate.clearCalled).toEqual(1);
            expect(formTemplate.removeCalled).toEqual([]);
        });

        it(`Should call template to clear items with empty array for ${name}`, () => {
            const value1 = [{
                value: 1,
            }, {
                value: 2,
            }];

            method(value1);
            method([]);

            expect(formArray.value).toEqual([]);
            expect(formTemplate.clearCalled).toEqual(1);
            expect(formTemplate.removeCalled).toEqual([]);
        });
    });

    it('should add control', () => {
        formArray.add();
        formArray.add();

        expect(formArray.value).toEqual([{
            value: null,
        }, {
            value: null,
        }]);
    });

    it('should call template when cleared', () => {
        formArray.add();
        formArray.clear();

        expect(formTemplate.clearCalled).toEqual(1);
    });

    it('should not call template when clearing empty form', () => {
        formArray.clear();

        expect(formTemplate.clearCalled).toEqual(0);
    });

    it('should call template when item removed', () => {
        formArray.add();
        formArray.removeAt(0);

        expect(formTemplate.removeCalled).toEqual([0]);
    });

    it('should not call template when item to remove out of bounds', () => {
        formArray.add();
        formArray.removeAt(1);

        expect(formTemplate.removeCalled).toEqual([]);
    });
});
