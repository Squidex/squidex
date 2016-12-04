/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { FocusOnInitDirective } from './focus-on-init.directive';

describe('FocusOnInitDirective', () => {
    let originalTimeout = 0;

    beforeEach(() => {
        originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;

        jasmine.DEFAULT_TIMEOUT_INTERVAL = 800;
    });

    it('should call focus on element when init', (done: any) => {
        const calledMethods: string[] = [];
        const calledElements: any[] = [];

        const renderer = {
            invokeElementMethod: (element: any, method: any, args: any) => {
                calledElements.push(element);
                calledMethods.push(method);
            }
        };

        const element: Ng2.ElementRef = {
            nativeElement: {}
        };

        new FocusOnInitDirective(element, renderer as Ng2.Renderer).ngOnInit();

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
