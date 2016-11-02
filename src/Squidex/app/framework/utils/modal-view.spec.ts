/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModalView } from './../';

describe('ModalView', () => {
    it('should have initial true value', () => {
        const dialog = new ModalView(true);

        checkValue(dialog, true);
    });

    it('should have initial false value', () => {
        const dialog = new ModalView(false);

        checkValue(dialog, false);
    });

    it('should become open after show', () => {
        const dialog = new ModalView(false);

        dialog.show();

        checkValue(dialog, true);
    });

    it('should become open after toggle', () => {
        const dialog = new ModalView(false);

        dialog.toggle();

        checkValue(dialog, true);
    });

    it('should become closed after hide', () => {
        const dialog = new ModalView(true);

        dialog.hide();

        checkValue(dialog, false);
    });

    it('should become closed after toggle', () => {
        const dialog = new ModalView(true);

        dialog.toggle();

        checkValue(dialog, false);
    });

    function checkValue(dialog: ModalView, expected: boolean) {
        let result: boolean | null = null;

        dialog.isOpenChanges.subscribe(value => {
            result = value;
        }).unsubscribe();

        expect(result).toBe(expected);
    }
});

