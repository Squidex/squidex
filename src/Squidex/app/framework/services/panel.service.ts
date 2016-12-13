/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable, Renderer } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export const PanelServiceFactory = () => {
    return new PanelService();
};

@Injectable()
export class PanelService {
    private readonly elements: any[] = [];
    private readonly changed$ = new Subject<number>();

    public get changed(): Observable<number> {
        return this.changed$;
    }

    public push(element: any, renderer: Renderer) {
        this.elements.push(element);
        this.update(renderer);
    }

    public pop(element: any, renderer: Renderer) {
        this.elements.splice(-1, 1);
        this.update(renderer);
    }

    private update(renderer: Renderer) {
        let currentPosition = 0;
        let currentLayer = this.elements.length * 10;

        for (let element of this.elements) {
            const width = element.getBoundingClientRect().width;

            renderer.setElementStyle(element, 'left', currentPosition + 'px');
            renderer.setElementStyle(element, 'z-index', currentLayer.toString());

            currentPosition += width;
            currentLayer -= 10;
        }

        this.changed$.next(currentPosition);
    }
}