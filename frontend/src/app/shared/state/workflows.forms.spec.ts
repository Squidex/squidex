/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { AddWorkflowDto } from '../model';
import { AddWorkflowForm } from './workflows.forms';

describe('AddWorkflowForm', () => {
    let form: AddWorkflowForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new AddWorkflowForm());
    });

    it('should have no name initially', () => {
        expect(form.hasNoName()).toBeTruthy();
    });

    it('should have name after value changed', () => {
        form.form.name().value.set('my-workflow');

        expect(form.hasNoName()).toBeFalsy();
    });

    it('should validate name length', () => {
        form.form.name().value.set('a'.repeat(41));

        expect(form.form().valid()).toBeFalsy();
        expect(form.form.name().errors().map(x => x.kind)).toEqual(['maxLength']);
    });

    it('should submit dto', () => {
        form.form.name().value.set('my-workflow');

        const result = form.getValue();

        expect(result).toEqual(new AddWorkflowDto({ name: 'my-workflow' }));
    });
});
