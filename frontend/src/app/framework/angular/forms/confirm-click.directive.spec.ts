/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService } from '@app/framework/internal';
import { ConfirmClickDirective } from './confirm-click.directive';

@Component({
    template: `
        <button
            sqxConfirmClick
            [confirmRememberKey]="confirmRememberKey()"
            [confirmRequired]="confirmRequired()"
            [confirmText]="confirmText()"
            [confirmTitle]="confirmTitle()">
        </button>
    `,
    imports: [
        ConfirmClickDirective,
    ],
})
class TestHostComponent {
    public readonly confirmRememberKey = signal<string | undefined>(undefined);
    public readonly confirmRequired = signal(true);
    public readonly confirmText = signal<string | undefined>(undefined);
    public readonly confirmTitle = signal<string | undefined>(undefined);
}

describe('ConfirmClickDirective', () => {
    let dialogs: IMock<DialogService>;
    let fixture: ComponentFixture<TestHostComponent>;
    let confirmClickDirective: ConfirmClickDirective;
    let button: HTMLButtonElement;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        TestBed.configureTestingModule({
            imports: [TestHostComponent],
            providers: [
                { provide: DialogService, useValue: dialogs.object },
            ],
        });

        fixture = TestBed.createComponent(TestHostComponent);

        const element = fixture.debugElement.query(By.directive(ConfirmClickDirective));

        confirmClickDirective = element.injector.get(ConfirmClickDirective);
        button = element.nativeElement;
    });

    it('Should invoke action directly when disabled', () => {
        setInputs(false, 'confirmText', 'confirmTitle', 'confirmKey');

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        button.click();

        expect(invoked).toBe(true);
        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });

    it('Should invoke action when confirmed', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        setInputs(true, 'confirmText', 'confirmTitle', 'confirmKey');

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        button.click();

        expect(invoked).toBe(true);
    });

    it('Should invoke action when unsubscribed in between', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        setInputs(true, 'confirmText', 'confirmTitle', 'confirmKey');

        let invoked = false;

        const subscription = confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        confirmClickDirective.beforeClick.subscribe(() => {
            subscription.unsubscribe();
        });

        button.click();

        expect(invoked).toBe(true);
    });

    it('Should not invoke action when not confirmed', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        setInputs(true, 'confirmText', 'confirmTitle', 'confirmKey');

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        button.click();

        expect(invoked).toBe(false);
    });

    it('Should invoke action when unsubscribed before', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        setInputs(true, 'confirmText', 'confirmTitle', 'confirmKey');

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        }).unsubscribe();

        button.click();

        expect(invoked).toBe(false);
    });

    it('Should not confirm when text is empty', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        setInputs(true, undefined, 'confirmTitle', undefined);

        button.click();

        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });

    it('Should not confirm when title is empty', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        setInputs(true, 'confirmText', undefined, undefined);

        button.click();

        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });

    function setInputs(confirmRequired: boolean, confirmText?: string, confirmTitle?: string, confirmRememberKey?: string) {
        const host = fixture.componentInstance;

        host.confirmRequired.set(confirmRequired);
        host.confirmText.set(confirmText);
        host.confirmTitle.set(confirmTitle);
        host.confirmRememberKey.set(confirmRememberKey);

        fixture.detectChanges();
    }
});
