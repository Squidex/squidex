/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { RootViewService } from './../';

describe('RootViewService', () => {
    const rootViewService = new RootViewService();

    it('should call view when clearing element', () => {
        let isClearCalled = false;

        const viewRef = {
            clear: () => {
                isClearCalled = true;
            }
        };

        rootViewService.init(<any>viewRef);
        rootViewService.clear();

        expect(isClearCalled).toBeTruthy();
    });

    it('should call view when creating element', () => {
        const view = {};

        const viewRef = {
            createEmbeddedView: () => {
                return view;
            }
        };

        rootViewService.init(<any>viewRef);

        const result = rootViewService.createEmbeddedView(<any>{});

        expect(result).toBe(view);
    });
});