/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { FocusOnChangeDirective } from './focus-on-change.directive';

describe('FocusOnChangeDirective', () => {
    let originalTimeout = 0;

    beforeEach(() => {
        originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;

        jasmine.DEFAULT_TIMEOUT_INTERVAL = 800;
    });

    it('should call focus on element when value changes', (done: any) => {
        let calledMethod: any;
        let calledElement: any;

        const renderer = {
            invokeElementMethod: (element: any, method: any, args: any) => {
                calledElement = element;
                calledMethod = method;
            }
        };

        const element: Ng2.ElementRef = {
            nativeElement: {}
        };

        new FocusOnChangeDirective(element, renderer as Ng2.Renderer).ngOnChanges({});

        expect(calledMethod).not.toBeDefined();
        expect(calledElement).not.toBeDefined();

        setTimeout(() => {
            expect(calledMethod).toBe('focus');
            expect(calledElement).toBe(element.nativeElement);

            done();
        }, 400);
    });

    afterEach(() => {
        jasmine.DEFAULT_TIMEOUT_INTERVAL = originalTimeout;
    });
});
