/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ToolbarService } from './toolbar.service';

describe('ToolbarService', () => {
    it('should instantiate', () => {
        const toolbarService = new ToolbarService();

        expect(toolbarService).toBeDefined();
    });

    it('should add button to toolbar', () => {
        const toolbarService = new ToolbarService();

        toolbarService.addButton(undefined, 'button1', () => { });
        toolbarService.addButton(undefined, 'button2', () => { });

        expect(toolbarService.buttons().map(x => x.name)).toEqual(['button1', 'button2']);
    });

    it('should replace button in toolbar', () => {
        const toolbarService = new ToolbarService();

        toolbarService.addButton(undefined, 'button1', () => { });
        toolbarService.addButton(undefined, 'button1', () => { }, { disabled: true });

        expect(toolbarService.buttons().length).toBe(1);
        expect(toolbarService.buttons()[0].disabled).toBe(true);
    });

    it('should not replace button in toolbar if nothing changed', () => {
        const toolbarService = new ToolbarService();

        const action = () => { };

        toolbarService.addButton(undefined, 'button1', action);

        const buttons = toolbarService.buttons();

        toolbarService.addButton(undefined, 'button1', action);

        expect(toolbarService.buttons()).toBe(buttons);
    });

    it('should remove buttons by owner', () => {
        const toolbarService = new ToolbarService();

        const owner1 = {};
        const owner2 = {};

        toolbarService.addButton(owner1, 'button1', () => { });
        toolbarService.addButton(owner2, 'button2', () => { });
        toolbarService.remove(owner1);

        expect(toolbarService.buttons().map(x => x.name)).toEqual(['button2']);
    });

    it('should remove all buttons', () => {
        const toolbarService = new ToolbarService();

        toolbarService.addButton(undefined, 'button1', () => { });
        toolbarService.addButton(undefined, 'button2', () => { });
        toolbarService.removeAll();

        expect(toolbarService.buttons().length).toBe(0);
    });
});
