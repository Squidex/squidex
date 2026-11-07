/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { required } from '@angular/forms/signals';
import { ErrorDto } from '@app/framework/internal';
import { SignalForm } from './signal-form';

type Model = { name: string; items: { key: string }[] };

class TestForm extends SignalForm<Model, { name: string; count: number }, Partial<Model> & { other?: string }> {
    constructor() {
        super({ name: '', items: [] }, path => {
            required(path.name);
        });
    }

    protected transformSubmit(value: Model) {
        return { name: value.name, count: value.items.length };
    }
}

describe('SignalForm', () => {
    let form: TestForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new TestForm());
    });

    it('should load only known keys', () => {
        form.load({ name: 'my-name', other: 'ignored' });

        expect(form.model()).toEqual({ name: 'my-name', items: [] });
    });

    it('should not submit if invalid', () => {
        const result = form.submit();

        expect(result).toBeNull();
        expect(form.form.name().touched()).toBeTruthy();
        expect(form.submitCount()).toBe(0);
    });

    it('should submit and disable form', () => {
        form.load({ name: 'my-name' });

        const result = form.submit();

        expect(result).toEqual({ name: 'my-name', count: 0 });
        expect(form.submitting()).toBeTruthy();
        expect(form.submitCount()).toBe(1);
        expect(form.form().disabled()).toBeTruthy();
        expect(form.form.name().disabled()).toBeTruthy();
    });

    it('should enable form and reset value when submit completed', () => {
        form.load({ name: 'my-name' });
        form.submit();
        form.submitCompleted({ newValue: { name: 'new-name' } });

        expect(form.model().name).toBe('new-name');
        expect(form.submitting()).toBeFalsy();
        expect(form.form().disabled()).toBeFalsy();
    });

    it('should set server errors on fields and remove them when value changes', () => {
        form.load({ name: 'my-name', items: [{ key: 'a' }] });
        form.submit();
        form.submitFailed(new ErrorDto(400, 'Validation failed', null, [
            'name: Name already taken.',
            'items[1].key: Key is invalid.',
        ]));

        expect(form.error()?.message).toBe('Validation failed');
        expect(form.form().disabled()).toBeFalsy();
        expect(form.form.name().errors()).toEqual([
            expect.objectContaining({ kind: 'custom', errors: ['Name already taken.'] }),
        ]);
        expect(form.form.items[0].key().errors()).toEqual([
            expect.objectContaining({ kind: 'custom', errors: ['Key is invalid.'] }),
        ]);

        form.form.name().value.set('other-name');

        expect(form.form.name().errors()).toEqual([]);
        expect(form.form.items[0].key().errors().length).toBe(1);
    });

    it('should submit when only server errors exist', () => {
        form.load({ name: 'my-name' });
        form.submit();
        form.submitFailed(new ErrorDto(400, 'Validation failed', null, [
            'name: Name already taken.',
        ]));

        const result = form.submit();

        expect(result).toEqual({ name: 'my-name', count: 0 });
    });
});
