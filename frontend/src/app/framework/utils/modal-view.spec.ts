/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogModel, ModalModel, Openable } from './modal-view';

describe('DialogModel', () => {
    it('should have default values', () => {
        const dialog = new DialogModel();

        checkValue(dialog, false);
    });

    it('should become open after show', () => {
        const dialog = new DialogModel();

        dialog.show();

        checkValue(dialog, true);
    });

    it('should become open after toggle', () => {
        const dialog = new DialogModel();

        dialog.toggle();

        checkValue(dialog, true);
    });

    it('should become closed after hide', () => {
        const dialog = new DialogModel().show();

        dialog.hide();

        checkValue(dialog, false);
    });

    it('should become closed after toggle', () => {
        const dialog = new DialogModel().show();

        dialog.toggle();

        checkValue(dialog, false);
    });

    it('should not hide other dialog', () => {
        const dialog1 = new DialogModel().show();
        const dialog2 = new DialogModel();

        dialog2.toggle();

        checkValue(dialog1, true);
        checkValue(dialog2, true);
    });
});

describe('ModalModel', () => {
    it('should have default values', () => {
        const modal = new ModalModel();

        checkValue(modal, false);
    });

    it('should become open after show', () => {
        const modal = new ModalModel();

        modal.show();

        checkValue(modal, true);
    });

    it('should become open after toggle', () => {
        const modal = new ModalModel();

        modal.toggle();

        checkValue(modal, true);
    });

    it('should become closed after hide', () => {
        const modal = new ModalModel().show();

        modal.hide();

        checkValue(modal, false);
    });

    it('should become closed after toggle', () => {
        const modal = new ModalModel().show();

        modal.toggle();

        checkValue(modal, false);
    });

    it('should hide other modal', () => {
        const modal1 = new ModalModel().show();
        const modal2 = new ModalModel();

        modal2.toggle();

        checkValue(modal1, false);
        checkValue(modal2, true);
    });
});

function checkValue(modal: Openable, expected: boolean) {
    let result: boolean;

    modal.isOpenChanges.subscribe(value => {
        result = value;
    }).unsubscribe();

    expect(result!).toBe(expected);
    expect(modal.isOpen).toBe(expected);
}
