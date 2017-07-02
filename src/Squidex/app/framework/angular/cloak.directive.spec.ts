/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { CloakDirective } from './cloak.directive';

describe('CloakDirective', () => {
    it('should remove class from element on ngOnInit', () => {
        let called = false;

        const element = {
            nativeElement: {}
        };

        const renderer = {
            setElementClass: (target: any, className: string, isAdd: boolean) => {
                called = true;

                expect(target).toBe(element.nativeElement);
                expect(className).toBe('sqx-cloak');
                expect(isAdd).toBeFalsy();
            }
        };

        new CloakDirective(<any>element, <any>renderer).ngOnInit();

        expect(called).toBeTruthy();
    });
});
