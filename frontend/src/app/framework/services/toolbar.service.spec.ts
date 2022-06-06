/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ButtonItem, ToolbarService } from './toolbar.service';

describe('ToolbarService', () => {
    it('should instantiate', () => {
        const toolbarService = new ToolbarService();

        expect(toolbarService).toBeDefined();
    });

    it('should add button to toolbar', () => {
        const toolbarService = new ToolbarService();

        let buttons: ReadonlyArray<ButtonItem>;
        let buttonsTriggered = 0;

        toolbarService.buttonsChanges.subscribe(result => {
            buttons = result;
            buttonsTriggered++;
        });

        toolbarService.addButton(undefined, 'button1', () => {});
        toolbarService.addButton(undefined, 'button2', () => {});

        expect(buttons!.length).toBe(2);
        expect(buttonsTriggered).toEqual(3);
    });

    it('should replace button in toolbar', () => {
        const toolbarService = new ToolbarService();

        let buttons: ReadonlyArray<ButtonItem>;
        let buttonsTriggered = 0;

        toolbarService.buttonsChanges.subscribe(result => {
            buttons = result;
            buttonsTriggered++;
        });

        toolbarService.addButton(undefined, 'button1', () => {});
        toolbarService.addButton(undefined, 'button1', () => {}, { disabled: true });

        expect(buttons!.length).toBe(1);
        expect(buttonsTriggered).toEqual(3);
    });

    it('should not replace button in toolbar if nothing changed', () => {
        const toolbarService = new ToolbarService();

        let buttons: ReadonlyArray<ButtonItem>;
        let buttonsTriggered = 0;

        toolbarService.buttonsChanges.subscribe(result => {
            buttons = result;
            buttonsTriggered++;
        });

        const action = () => {};

        toolbarService.addButton(undefined, 'button1', action);
        toolbarService.addButton(undefined, 'button1', action);

        expect(buttons!.length).toBe(1);
        expect(buttonsTriggered).toEqual(2);
    });

    it('should remove buttons by owner', () => {
        const toolbarService = new ToolbarService();

        let buttons: ReadonlyArray<ButtonItem>;
        let buttonsTriggered = 0;

        const owner1 = {};
        const owner2 = {};

        toolbarService.buttonsChanges.subscribe(result => {
            buttons = result;
            buttonsTriggered++;
        });

        toolbarService.addButton(owner1, 'button1', () => {});
        toolbarService.addButton(owner2, 'button2', () => {});
        toolbarService.remove(owner1);

        expect(buttons!.length).toBe(1);
        expect(buttonsTriggered).toEqual(4);
    });

    it('should remove all buttons', () => {
        const toolbarService = new ToolbarService();

        let buttons: ReadonlyArray<ButtonItem>;
        let buttonsTriggered = 0;

        toolbarService.buttonsChanges.subscribe(result => {
            buttons = result;
            buttonsTriggered++;
        });

        toolbarService.addButton(undefined, 'button1', () => {});
        toolbarService.addButton(undefined, 'button2', () => {});
        toolbarService.removeAll();

        expect(buttons!.length).toBe(0);
        expect(buttonsTriggered).toEqual(4);
    });
});
