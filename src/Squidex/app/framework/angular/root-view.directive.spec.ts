/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { RootViewDirective } from './../';

/* tslint:disable:no-unused-expression */

describe('RootViewDirective', () => {
    it('should call init of service in ctor', () => {
        let viewRef = {};
        let viewRefPassed: any = null;

        const service = {
            init: (ref: any) => {
                viewRefPassed = ref;
            }
        };

        new RootViewDirective(<any>viewRef, <any>service);

        expect(viewRef).toBe(viewRefPassed);
    });
});