/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { required } from '@angular/forms/signals';
import { of, Subject, throwError } from 'rxjs';
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

    it('should not call action if invalid', async () => {
        const action = vi.fn(() => of(1));

        const result = await form.submit(action);

        expect(result).toBeFalsy();
        expect(action).not.toHaveBeenCalled();
        expect(form.form.name().touched()).toBeTruthy();
        expect(form.submitCount()).toBe(1);
    });

    it('should disable form while submitting', async () => {
        const subject = new Subject<number>();

        form.load({ name: 'my-name' });

        const promise = form.submit(() => subject);

        expect(form.submitting()).toBeTruthy();
        expect(form.form().disabled()).toBeTruthy();
        expect(form.form.name().disabled()).toBeTruthy();

        subject.next(1);
        subject.complete();

        expect(await promise).toBeTruthy();
        expect(form.submitting()).toBeFalsy();
        expect(form.form().disabled()).toBeFalsy();
    });

    it('should pass transformed value to action', async () => {
        const action = vi.fn(() => Promise.resolve(1));

        form.load({ name: 'my-name' });

        await form.submit(action);

        expect(action).toHaveBeenCalledWith({ name: 'my-name', count: 0 });
    });

    it('should keep value after submit', async () => {
        form.load({ name: 'my-name' });
        form.form.name().value.set('new-name');
        form.form.name().markAsDirty();

        await form.submit(() => of(1));

        expect(form.model().name).toBe('new-name');
        expect(form.form().dirty()).toBeFalsy();
    });

    it('should reset value after submit', async () => {
        form.load({ name: 'my-name' });

        await form.submit(() => of(1), { resetAfterSubmit: true });

        expect(form.model().name).toBe('');
    });

    it('should update value with result', async () => {
        form.load({ name: 'my-name', items: [{ key: 'a' }] });

        await form.submitAndUpdate(() => of({ name: 'new-name' }));

        expect(form.model()).toMatchObject({ name: 'new-name', items: [{ key: 'a' }] });
    });

    it('should reset and update value with result', async () => {
        form.load({ name: 'my-name', items: [{ key: 'a' }] });

        await form.submitAndUpdate(() => of({ name: 'new-name' }), { resetAfterSubmit: true });

        expect(form.model()).toEqual({ name: 'new-name', items: [] });
    });

    it('should set server errors on fields and remove them when value changes', async () => {
        form.load({ name: 'my-name', items: [{ key: 'a' }] });

        const result = await form.submit(() => throwError(() => new ErrorDto(400, 'Validation failed', null, [
            'name: Name already taken.',
            'items[1].key: Key is invalid.',
        ])));

        expect(result).toBeFalsy();
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

    it('should set error from string', async () => {
        form.load({ name: 'my-name' });

        await form.submit(() => Promise.reject('Failed'));

        expect(form.error()?.message).toBe('Failed');
    });

    it('should not submit while server errors exist', async () => {
        form.load({ name: 'my-name' });

        await form.submit(() => throwError(() => new ErrorDto(400, 'Validation failed', null, [
            'name: Name already taken.',
        ])));

        const action = vi.fn(() => of(1));

        await form.submit(action);

        expect(action).not.toHaveBeenCalled();
    });
});
