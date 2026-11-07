/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { FormField, required } from '@angular/forms/signals';
import { LocalizerService } from '@app/framework/internal';
import { ControlErrorsComponent } from './control-errors.component';
import { SignalForm } from './signal-form';
import { SignalSelectDirective } from './signal-select.directive';

type Model = { name: string; teamId: string | null };

class TestForm extends SignalForm<Model, Model> {
    constructor() {
        super({ name: '', teamId: null }, path => {
            required(path.name);
        });
    }
}

@Component({
    imports: [
        ControlErrorsComponent,
        FormField,
        FormsModule,
        SignalSelectDirective,
    ],
    template: `
        <sqx-control-errors [for]="form.form.name" />
        <input [formField]="form.form.name" />
        <select [formField]="form.form.teamId">
            <option [ngValue]="null">-</option>
            <option [ngValue]="'team1'">Team 1</option>
        </select>
    `,
})
class TestHostComponent {
    public readonly form = new TestForm();
}

describe('SignalForm controls', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                { provide: LocalizerService, useValue: new LocalizerService({ 'validation.required': '{field|upper} is required.' }) },
            ],
        });
    });

    it('should show errors when field is touched', async () => {
        const fixture = TestBed.createComponent(TestHostComponent);
        fixture.detectChanges();

        const element: HTMLElement = fixture.nativeElement;
        expect(element.textContent).not.toContain('is required');

        await fixture.componentInstance.form.submit(() => {
            return Promise.resolve();
        });
        fixture.detectChanges();

        expect(element.textContent).toContain('Name is required.');
    });

    it('should bind select with ngValue options', () => {
        const fixture = TestBed.createComponent(TestHostComponent);
        fixture.detectChanges();

        const select: HTMLSelectElement = fixture.nativeElement.querySelector('select');
        select.selectedIndex = 1;
        select.dispatchEvent(new Event('change'));
        fixture.detectChanges();

        expect(fixture.componentInstance.form.model().teamId).toBe('team1');

        fixture.componentInstance.form.load({ teamId: null });
        fixture.detectChanges();

        expect(select.selectedIndex).toBe(0);
    });
});
