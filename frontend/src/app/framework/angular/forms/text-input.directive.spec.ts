/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import { TextInputDirective } from './text-input.directive';

@Component({
    template: `<input [formField]="form.text" [sqxTextInput]="transform()" />`,
    imports: [FormField, TextInputDirective],
})
class TestHostComponent {
    public readonly isDisabled = signal(false);

    public readonly transform = signal<string | null>(null);

    public readonly model = signal<{ text: string | null }>({ text: null });

    public readonly form = form(this.model, path => {
        disabled(path.text,
            {when: () => this.isDisabled() }
        );

        required(path.text);
    });
}

describe('TextInputDirective', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [TestHostComponent],
        });
    });

    it('should render null as empty text', () => {
        const { input } = render();

        expect(input.value).toEqual('');
    });

    it('should render value from model', () => {
        const { fixture, input } = render();

        fixture.componentInstance.model.set({ text: 'Hello' });
        fixture.detectChanges();

        expect(input.value).toEqual('Hello');
    });

    it('should update model with text', () => {
        const { fixture, input } = render();

        enterText(fixture, input, 'Hello');

        expect(fixture.componentInstance.model().text).toEqual('Hello');
        expect(fixture.componentInstance.form.text().errors()).toEqual([]);
    });

    it('should keep digits as text if value was null', () => {
        const { fixture, input } = render();

        enterText(fixture, input, '42');

        expect(fixture.componentInstance.model().text).toEqual('42');
    });

    it('should mark field as touched on blur', () => {
        const { fixture, input } = render();

        input.dispatchEvent(new Event('blur'));
        fixture.detectChanges();

        expect(fixture.componentInstance.form.text().touched()).toBeTruthy();
    });

    it('should disable input', () => {
        const { fixture, input } = render();

        fixture.componentInstance.isDisabled.set(true);
        fixture.detectChanges();

        expect(input.disabled).toBeTruthy();
    });

    it('should transform text', () => {
        const { fixture, input } = render();

        fixture.componentInstance.transform.set('Slugify');
        fixture.detectChanges();

        enterText(fixture, input, 'Hello World');

        expect(input.value).toEqual('hello-world');
        expect(fixture.componentInstance.model().text).toEqual('hello-world');
    });

    it('should forward native properties', () => {
        const { input } = render();

        expect(input.required).toBeTruthy();
    });

    function render() {
        const fixture = TestBed.createComponent(TestHostComponent);
        fixture.detectChanges();

        const input: HTMLInputElement = fixture.nativeElement.querySelector('input');

        return { fixture, input };
    }

    function enterText(fixture: ReturnType<typeof render>['fixture'], input: HTMLInputElement, text: string) {
        input.value = text;
        input.dispatchEvent(new Event('input'));
        fixture.detectChanges();
    }
});
