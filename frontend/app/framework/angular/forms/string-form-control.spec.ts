/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { StringFormControl } from './string-form-control';

describe('StringFormControl', () => {
    it('should convert empty string to undefined', () => {
        const formControl = new StringFormControl();

        formControl.setValue('');

        expect(formControl.value).toBeUndefined();
    });

    it('should convert empty string to undefined when patching', () => {
        const formControl = new StringFormControl();

        formControl.patchValue('');

        expect(formControl.value).toBeUndefined();
    });
});