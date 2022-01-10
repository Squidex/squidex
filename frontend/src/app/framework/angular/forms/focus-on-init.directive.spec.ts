/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ElementRef } from '@angular/core';
import { FocusOnInitDirective } from './focus-on-init.directive';

describe('FocusOnInitDirective', () => {
    let isFocusCalled = false;
    let isSelectCalled = false;

    const element: ElementRef = {
        nativeElement: {
            focus: () => {
                isFocusCalled = true;
            },
            select: () => {
                isSelectCalled = true;
            },
        },
    };

    beforeEach(() => {
        isFocusCalled = false;
        isSelectCalled = false;
    });

    it('should call focus on element if init', (cb) => {
        const directive = new FocusOnInitDirective(element);
        directive.select = false;
        directive.ngAfterViewInit();

        setTimeout(() => {
            expect(isFocusCalled).toBeTruthy();
            expect(isSelectCalled).toBeFalsy();

            cb();
        }, 200);
    });

    it('should call select on element if init', (cb) => {
        const directive = new FocusOnInitDirective(element);
        directive.select = true;
        directive.ngAfterViewInit();

        setTimeout(() => {
            expect(isFocusCalled).toBeTruthy();
            expect(isSelectCalled).toBeTruthy();

            cb();
        }, 200);
    });
});
