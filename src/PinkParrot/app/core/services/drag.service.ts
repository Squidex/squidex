/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Subject, Observable } from 'rxjs';

import { Vec2 } from './../utils/vec2';

export interface DropEvent { position: Vec2; model: any; dropTarget: string; }

export const DragServiceFactory = () => {
    return new DragService();
};

export class DragService {
    private readonly dropEvent = new Subject<DropEvent>();

    public get drop(): Observable<DropEvent> {
        return this.dropEvent;
    }

    public emitDrop(event: DropEvent) {
        this.dropEvent.next(event);
    }
}