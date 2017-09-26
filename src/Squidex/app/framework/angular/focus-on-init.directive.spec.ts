/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ElementRef, Renderer } from '@angular/core';

import { FocusOnInitDirective } from './focus-on-init.directive';

describe('FocusOnInitDirective', () => {
    it('should call focus on element when init', () => {
        const calledMethods: string[] = [];
        const calledElements: any[] = [];

        const renderer = {
            invokeElementMethod: (elem: any, method: any) => {
                calledElements.push(elem);
                calledMethods.push(method);
            }
        };

        const element: ElementRef = {
            nativeElement: {}
        };

        const directive = new FocusOnInitDirective(element, renderer as Renderer);
        directive.select = true;
        directive.ngAfterViewInit();

        expect(calledMethods).toEqual(['focus', 'select']);
        expect(calledElements).toEqual([element.nativeElement, element.nativeElement]);
    });
});
