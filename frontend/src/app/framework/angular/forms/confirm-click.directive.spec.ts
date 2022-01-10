/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { DialogService } from '@app/framework/internal';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { ConfirmClickDirective } from './confirm-click.directive';

describe('ConfirmClickDirective', () => {
    let dialogs: IMock<DialogService>;
    let confirmClickDirective: ConfirmClickDirective;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        confirmClickDirective = new ConfirmClickDirective(dialogs.object);
    });

    it('Should invoke action directly when disabled', () => {
        confirmClickDirective.confirmRequired = false;
        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.confirmRememberKey = 'confirmKey';

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        confirmClickDirective.onClick(new Event('click'));

        expect(invoked).toBeTrue();

        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });

    it('Should invoke action when confirmed', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.confirmRememberKey = 'confirmKey';

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        confirmClickDirective.onClick(new Event('click'));

        expect(invoked).toBeTrue();
    });

    it('Should invoke action when unsubscribed in between', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.confirmRememberKey = 'confirmKey';

        let invoked = false;

        const subscription = confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        confirmClickDirective.beforeClick.subscribe(() => {
            subscription.unsubscribe();
        });

        confirmClickDirective.onClick(new Event('click'));

        expect(invoked).toBeTrue();
    });

    it('Should not invoke action when not confirmed', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.confirmRememberKey = 'confirmKey';

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        });

        confirmClickDirective.onClick(new Event('click'));

        expect(invoked).toBeFalse();
    });

    it('Should invoke action when unsubscribed before', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(true));

        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.confirmRememberKey = 'confirmKey';

        let invoked = false;

        confirmClickDirective.clickConfirmed.subscribe(() => {
            invoked = true;
        }).unsubscribe();

        confirmClickDirective.onClick(new Event('click'));

        expect(invoked).toBeFalse();
    });

    it('Should not confirm when text is empty', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        confirmClickDirective.confirmTitle = 'confirmTitle';
        confirmClickDirective.onClick(new Event('click'));

        expect().nothing();

        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });

    it('Should not confirm when title is empty', () => {
        dialogs.setup(x => x.confirm('confirmTitle', 'confirmText', 'confirmKey'))
            .returns(() => of(false));

        confirmClickDirective.confirmText = 'confirmText';
        confirmClickDirective.onClick(new Event('click'));

        expect().nothing();

        dialogs.verify(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAny()), Times.never());
    });
});
