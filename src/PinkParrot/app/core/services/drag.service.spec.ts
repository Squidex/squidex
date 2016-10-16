/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { 
    DragService, 
    DragServiceFactory,
    DropEvent 
} from './../';

describe('DragService', () => {
    it('should instantiate from factory', () => {
        const dragService = DragServiceFactory();

        expect(dragService).toBeDefined();
    });

    it('should instantiate', () => {
        const dragService = new DragService();

        expect(dragService).toBeDefined();
    });

    it('should raise event handler when dropped', () => {
        let emittedEvent: DropEvent | null = null;

        const dragService = new DragService();

        dragService.drop.subscribe(e => {
            emittedEvent = e;
        });

        const event: DropEvent = { position: null!, model: null!, dropTarget: null! };

        dragService.emitDrop(event);

        expect(emittedEvent).toBe(event);
    });
});