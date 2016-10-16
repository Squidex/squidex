/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { CloakDirective } from './cloak.directive';

describe('CloakDirective', () => {
    it('should remove class from element on ngOnInit', () => {
        let called = false;

        const element = {
            nativeElement: {
                classList: {
                    remove: () => {
                        called = true;
                    }
                }
            }
        };
    
         new CloakDirective(element).ngOnInit();

        expect(called).toBeTruthy();
    });
});
