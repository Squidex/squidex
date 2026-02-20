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

    it('should call focus on element if init', async () => {
        const directive = new FocusOnInitDirective(element);
        directive.scheduler = action => action();
        directive.select = false;
        directive.ngAfterViewInit();

        expect(isFocusCalled).toBeTruthy();
        expect(isSelectCalled).toBeFalsy();
    });

    it('should call select on element if init', async () => {
        const directive = new FocusOnInitDirective(element);
        directive.scheduler = action => action();
        directive.select = true;
        directive.ngAfterViewInit();

        expect(isFocusCalled).toBeTruthy();
        expect(isSelectCalled).toBeTruthy();
    });
});
