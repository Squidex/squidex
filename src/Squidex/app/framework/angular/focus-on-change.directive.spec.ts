/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ElementRef, Renderer } from '@angular/core';

import { FocusOnChangeDirective } from './focus-on-change.directive';

describe('FocusOnChangeDirective', () => {
    let originalTimeout = 0;

    beforeEach(() => {
        originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;

        jasmine.DEFAULT_TIMEOUT_INTERVAL = 800;
    });

    it('should call focus on element when value changes', (done: any) => {
        const calledMethods: string[] = [];
        const calledElements: any[] = [];

        const renderer = {
            invokeElementMethod: (element: any, method: any, args: any) => {
                calledElements.push(element);
                calledMethods.push(method);
            }
        };

        const element: ElementRef = {
            nativeElement: {}
        };

        new FocusOnChangeDirective(element, renderer as Renderer).ngOnChanges({});

        expect(calledMethods).toEqual([]);

        setTimeout(() => {
            expect(calledMethods).toEqual(['focus', 'select']);
            expect(calledElements).toEqual([element.nativeElement, element.nativeElement]);

            done();
        }, 400);
    });

    afterEach(() => {
        jasmine.DEFAULT_TIMEOUT_INTERVAL = originalTimeout;
    });
});
