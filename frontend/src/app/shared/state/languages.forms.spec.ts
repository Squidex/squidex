/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { EditLanguageForm } from './languages.forms';

describe('EditLanguageForm', () => {
    let form: EditLanguageForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new EditLanguageForm());
    });

    it('should reset optional flag if language becomes master', () => {
        form.load({ isMaster: false, isOptional: true });
        TestBed.tick();

        form.form.isMaster().value.set(true);
        TestBed.tick();

        expect(form.model()).toEqual({ isMaster: true, isOptional: false });
    });

    it('should keep optional flag if language is not master', () => {
        form.load({ isMaster: false, isOptional: true });
        TestBed.tick();

        expect(form.model()).toEqual({ isMaster: false, isOptional: true });
    });
});
